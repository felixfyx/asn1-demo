using System;
using System.Collections;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Reflection;

namespace ASN1Demo
{
    /// <summary>
    /// Unified ASN.1 serializer combining dynamic property discovery with version compatibility.
    /// Supports backward/forward compatibility, optional properties, default values, and version management.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class Asn1PropertyAttribute : Attribute
    {
        public int Order { get; set; }
        public bool Ignore { get; set; }
        public UniversalTagNumber? TagNumber { get; set; }

        // Version compatibility features

        // Set default to true so that you won't crash easily unless
        // you explicitly want to make a property mandatory.
        public bool Optional { get; set; } = true;
        public object? DefaultValue { get; set; }
        public string? SinceVersion { get; set; }
        public string? RemovedInVersion { get; set; }

        public Asn1PropertyAttribute(int order = 0)
        {
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class Asn1SerializableAttribute : Attribute
    {
        public bool UseSequence { get; set; } = true;
        public string Version { get; set; } = "1.0";
        public bool IncludeVersionMetadata { get; set; } = true;
    }

    public static class Asn1Serializer
    {
        private static readonly Dictionary<Type, UniversalTagNumber> TypeToTagMap = new()
        {
            { typeof(string), UniversalTagNumber.UTF8String },
            { typeof(int), UniversalTagNumber.Integer },
            { typeof(long), UniversalTagNumber.Integer },
            { typeof(bool), UniversalTagNumber.Boolean },
            { typeof(double), UniversalTagNumber.UTF8String },
            { typeof(float), UniversalTagNumber.UTF8String },
            { typeof(DateTime), UniversalTagNumber.UTF8String }
        };

        // TODO: Serialize and save file
        /// <summary>
        /// Serialize an object to ASN.1 format with optional version targeting
        /// </summary>
        public static byte[] Serialize<T>(T obj, string? targetVersion = null) where T : class
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var writer = new AsnWriter(AsnEncodingRules.DER);
            var type = obj.GetType();
            var serializableAttr = type.GetCustomAttribute<Asn1SerializableAttribute>();

            /*
            ?   -> Nullable
            ??  -> If null, use the right side
            */
            bool useSequence = serializableAttr?.UseSequence ?? true;
            bool includeVersion = serializableAttr?.IncludeVersionMetadata ?? true;
            string version = targetVersion ?? serializableAttr?.Version ?? "1.0";

            if (useSequence)
            {
                using (writer.PushSequence())
                {
                    if (includeVersion)
                    {
                        writer.WriteCharacterString(UniversalTagNumber.UTF8String, version);
                        writer.WriteCharacterString(UniversalTagNumber.UTF8String, type.FullName ?? type.Name);
                    }
                    SerializeProperties(writer, obj, type, version);
                }
            }
            else
            {
                SerializeProperties(writer, obj, type, version);
            }

            return writer.Encode();
        }

        /// <summary>
        /// Deserialize ASN.1 data to an object with version compatibility support
        /// </summary>
        public static T Deserialize<T>(byte[] data, string? expectedVersion = null) where T : class, new()
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            var reader = new AsnReader(data, AsnEncodingRules.DER);
            var type = typeof(T);
            var obj = new T();
            var serializableAttr = type.GetCustomAttribute<Asn1SerializableAttribute>();

            bool useSequence = serializableAttr?.UseSequence ?? true;
            bool includeVersion = serializableAttr?.IncludeVersionMetadata ?? true;

            string? dataVersion = null;
            string? dataType = null;

            if (useSequence)
            {
                var sequence = reader.ReadSequence();

                if (includeVersion && sequence.HasData)
                {
                    try
                    {
                        dataVersion = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);
                        if (sequence.HasData)
                        {
                            dataType = sequence.ReadCharacterString(UniversalTagNumber.UTF8String);
                        }
                    }
                    catch
                    {
                        // If version reading fails, assume no version metadata
                        sequence = new AsnReader(data, AsnEncodingRules.DER).ReadSequence();
                        dataVersion = null;
                        dataType = null;
                    }
                }

                DeserializeProperties(sequence, obj, type, dataVersion, expectedVersion);
            }
            else
            {
                DeserializeProperties(reader, obj, type, dataVersion, expectedVersion);
            }

            ApplyDefaultValues(obj, type, dataVersion);
            return obj;
        }

        private static void SerializeProperties(AsnWriter writer, object obj, Type type, string version)
        {
            var properties = GetSerializableProperties(type, version, SerializationMode.Serialize);

            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                var attr = prop.GetCustomAttribute<Asn1PropertyAttribute>();

                // Skip null optional properties
                if (value == null && (attr?.Optional == true))
                {
                    continue;
                }

                SerializeProperty(writer, prop, value);
            }
        }

        private static void DeserializeProperties(AsnReader reader, object obj, Type type, string? dataVersion, string? expectedVersion)
        {
            var targetProperties = GetSerializableProperties(type, expectedVersion, SerializationMode.Deserialize);
            var dataProperties = GetSerializableProperties(type, dataVersion, SerializationMode.Serialize);

            // Create mapping between data positions and target properties
            var propertyMapping = new Dictionary<int, PropertyInfo>();

            foreach (var targetProp in targetProperties)
            {
                // Find corresponding property in data version by name
                var dataProp = dataProperties.FirstOrDefault(p => p.Name == targetProp.Name);
                if (dataProp != null)
                {
                    var dataAttr = dataProp.GetCustomAttribute<Asn1PropertyAttribute>();
                    var dataOrder = dataAttr?.Order ?? 0;
                    propertyMapping[dataOrder] = targetProp;
                }
            }

            // Process properties in data order
            var dataIndex = 0;
            while (reader.HasData)
            {
                var currentDataProp = dataIndex < dataProperties.Length ? dataProperties[dataIndex] : null;
                var currentDataOrder = currentDataProp?.GetCustomAttribute<Asn1PropertyAttribute>()?.Order ?? dataIndex;

                if (propertyMapping.TryGetValue(currentDataOrder, out var targetProp))
                {
                    var attr = targetProp.GetCustomAttribute<Asn1PropertyAttribute>();

                    try
                    {
                        // Check if this property should be available in the target version
                        if (IsPropertyValidForVersion(targetProp, expectedVersion, SerializationMode.Deserialize))
                        {
                            var value = DeserializeProperty(reader, targetProp);
                            targetProp.SetValue(obj, value);
                        }
                        else
                        {
                            SkipNextValue(reader);
                        }
                    }
                    catch (Exception ex)
                    {
                        if (attr?.Optional == true || IsVersionCompatibilityIssue(targetProp, dataVersion, expectedVersion))
                        {
                            SkipNextValue(reader);
                        }
                        else
                        {
                            throw new InvalidOperationException($"Failed to deserialize property {targetProp.Name}: {ex.Message}", ex);
                        }
                    }
                }
                else
                {
                    // Property exists in data but not in target version - skip it
                    SkipNextValue(reader);
                }

                dataIndex++;
            }
        }

        private static void SerializeProperty(AsnWriter writer, PropertyInfo prop, object? value)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var propType = prop.PropertyType;
            var attr = prop.GetCustomAttribute<Asn1PropertyAttribute>();
            var tagNumber = attr?.TagNumber ?? GetDefaultTag(propType);

            if (propType == typeof(string))
            {
                writer.WriteCharacterString(tagNumber, (string)value);
            }
            else if (propType == typeof(int) || propType == typeof(long))
            {
                writer.WriteInteger(Convert.ToInt64(value));
            }
            else if (propType == typeof(bool))
            {
                writer.WriteBoolean((bool)value);
            }
            else if (propType == typeof(double) || propType == typeof(float))
            {
                writer.WriteCharacterString(tagNumber, value.ToString() ?? "");
            }
            else if (propType == typeof(DateTime))
            {
                writer.WriteCharacterString(tagNumber, ((DateTime)value).ToString("O"));
            }
            else if (propType.IsEnum)
            {
                // Serialize enum as its underlying integer value
                var underlyingValue = Convert.ToInt64(value);
                writer.WriteInteger(underlyingValue);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
            {
                SerializeCollection(writer, (IEnumerable)value, propType);
            }
            else if (propType.IsClass)
            {
                var serializedChild = Serialize(value);
                writer.WriteOctetString(serializedChild);
            }
            else
            {
                writer.WriteCharacterString(tagNumber, value.ToString() ?? "");
            }
        }

        private static void SerializeCollection(AsnWriter writer, IEnumerable collection, Type collectionType)
        {
            using (writer.PushSequence())
            {
                foreach (var item in collection)
                {
                    if (item == null)
                    {
                        writer.WriteNull();
                        continue;
                    }

                    var itemType = item.GetType();
                    if (itemType == typeof(string))
                    {
                        writer.WriteCharacterString(UniversalTagNumber.UTF8String, (string)item);
                    }
                    else if (itemType.IsValueType || itemType.IsPrimitive)
                    {
                        var tagNumber = GetDefaultTag(itemType);
                        if (itemType == typeof(int) || itemType == typeof(long))
                        {
                            writer.WriteInteger(Convert.ToInt64(item));
                        }
                        else if (itemType == typeof(bool))
                        {
                            writer.WriteBoolean((bool)item);
                        }
                        else if (itemType.IsEnum)
                        {
                            // Serialize enum as its underlying integer value
                            var underlyingValue = Convert.ToInt64(item);
                            writer.WriteInteger(underlyingValue);
                        }
                        else
                        {
                            writer.WriteCharacterString(tagNumber, item.ToString() ?? "");
                        }
                    }
                    else
                    {
                        var serializedItem = Serialize(item);
                        writer.WriteOctetString(serializedItem);
                    }
                }
            }
        }

        private static object? DeserializeProperty(AsnReader reader, PropertyInfo prop)
        {
            if (!reader.HasData) return null;

            var propType = prop.PropertyType;

            var tag = reader.PeekTag();
            if (tag.TagClass == TagClass.Universal && tag.TagValue == (int)UniversalTagNumber.Null)
            {
                reader.ReadNull();
                return null;
            }

            if (propType == typeof(string))
            {
                return reader.ReadCharacterString(UniversalTagNumber.UTF8String);
            }
            else if (propType == typeof(int))
            {
                return (int)reader.ReadInteger();
            }
            else if (propType == typeof(long))
            {
                return reader.ReadInteger();
            }
            else if (propType == typeof(bool))
            {
                return reader.ReadBoolean();
            }
            else if (propType == typeof(double))
            {
                var str = reader.ReadCharacterString(UniversalTagNumber.UTF8String);
                return double.Parse(str);
            }
            else if (propType == typeof(float))
            {
                var str = reader.ReadCharacterString(UniversalTagNumber.UTF8String);
                return float.Parse(str);
            }
            else if (propType == typeof(DateTime))
            {
                var str = reader.ReadCharacterString(UniversalTagNumber.UTF8String);
                return DateTime.Parse(str);
            }
            else if (propType.IsEnum)
            {
                // Deserialize enum from its underlying integer value
                var longValue = reader.ReadInteger();
                // Convert to int for most common enum underlying type
                var intValue = (int)longValue;
                return Enum.ToObject(propType, intValue);
            }
            else if (typeof(IEnumerable).IsAssignableFrom(propType) && propType != typeof(string))
            {
                return DeserializeCollection(reader, propType);
            }
            else if (propType.IsClass)
            {
                var childData = reader.ReadOctetString();
                var method = typeof(Asn1Serializer).GetMethod(nameof(Deserialize))?.MakeGenericMethod(propType);
                return method?.Invoke(null, new object[] { childData, null! });
            }
            else
            {
                var str = reader.ReadCharacterString(UniversalTagNumber.UTF8String);
                return Convert.ChangeType(str, propType);
            }
        }

        private static object? DeserializeCollection(AsnReader reader, Type collectionType)
        {
            var sequence = reader.ReadSequence();

            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elementType = collectionType.GetGenericArguments()[0];
                var listType = typeof(List<>).MakeGenericType(elementType);
                var list = (IList?)Activator.CreateInstance(listType);

                if (list != null)
                {
                    while (sequence.HasData)
                    {
                        var item = DeserializeCollectionItem(sequence, elementType);
                        list.Add(item);
                    }
                }

                return list;
            }

            return null;
        }

        private static object? DeserializeCollectionItem(AsnReader reader, Type itemType)
        {
            if (!reader.HasData) return null;

            var tag = reader.PeekTag();
            if (tag.TagClass == TagClass.Universal && tag.TagValue == (int)UniversalTagNumber.Null)
            {
                reader.ReadNull();
                return null;
            }

            if (itemType == typeof(string))
            {
                return reader.ReadCharacterString(UniversalTagNumber.UTF8String);
            }
            else if (itemType == typeof(int))
            {
                return (int)reader.ReadInteger();
            }
            else if (itemType == typeof(bool))
            {
                return reader.ReadBoolean();
            }
            else if (itemType.IsEnum)
            {
                // Deserialize enum from its underlying integer value
                var longValue = reader.ReadInteger();
                // Convert to int for most common enum underlying type
                var intValue = (int)longValue;
                return Enum.ToObject(itemType, intValue);
            }
            else if (itemType.IsClass)
            {
                var childData = reader.ReadOctetString();
                var method = typeof(Asn1Serializer).GetMethod(nameof(Deserialize))?.MakeGenericMethod(itemType);
                return method?.Invoke(null, new object[] { childData, null! });
            }
            else
            {
                var str = reader.ReadCharacterString(UniversalTagNumber.UTF8String);
                return Convert.ChangeType(str, itemType);
            }
        }

        private static void SkipNextValue(AsnReader reader)
        {
            try
            {
                var tag = reader.PeekTag();

                if (tag.TagClass == TagClass.Universal)
                {
                    switch ((UniversalTagNumber)tag.TagValue)
                    {
                        case UniversalTagNumber.Boolean:
                            reader.ReadBoolean();
                            break;
                        case UniversalTagNumber.Integer:
                            reader.ReadInteger();
                            break;
                        case UniversalTagNumber.UTF8String:
                            reader.ReadCharacterString(UniversalTagNumber.UTF8String);
                            break;
                        case UniversalTagNumber.Sequence:
                            reader.ReadSequence();
                            break;
                        case UniversalTagNumber.OctetString:
                            reader.ReadOctetString();
                            break;
                        case UniversalTagNumber.Null:
                            reader.ReadNull();
                            break;
                        default:
                            reader.ReadEncodedValue();
                            break;
                    }
                }
                else
                {
                    reader.ReadEncodedValue();
                }
            }
            catch
            {
                return;
            }
        }

        private static bool IsVersionCompatibilityIssue(PropertyInfo prop, string? dataVersion, string? expectedVersion)
        {
            var attr = prop.GetCustomAttribute<Asn1PropertyAttribute>();
            if (attr == null) return false;

            if (!string.IsNullOrEmpty(attr.SinceVersion) && !string.IsNullOrEmpty(dataVersion))
            {
                return CompareVersions(dataVersion, attr.SinceVersion) < 0;
            }

            if (!string.IsNullOrEmpty(attr.RemovedInVersion) && !string.IsNullOrEmpty(dataVersion))
            {
                return CompareVersions(dataVersion, attr.RemovedInVersion) >= 0;
            }

            return false;
        }

        private static int CompareVersions(string version1, string version2)
        {
            try
            {
                var v1 = new Version(version1);
                var v2 = new Version(version2);
                return v1.CompareTo(v2);
            }
            catch
            {
                return string.Compare(version1, version2, StringComparison.OrdinalIgnoreCase);
            }
        }

        private static void ApplyDefaultValues(object obj, Type type, string? dataVersion)
        {
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => p.CanRead && p.CanWrite);

            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttribute<Asn1PropertyAttribute>();
                if (attr?.DefaultValue != null)
                {
                    var currentValue = prop.GetValue(obj);

                    if (IsDefaultValue(currentValue, prop.PropertyType))
                    {
                        try
                        {
                            var defaultValue = Convert.ChangeType(attr.DefaultValue, prop.PropertyType);
                            prop.SetValue(obj, defaultValue);
                        }
                        catch
                        {
                            // Ignore conversion errors for default values
                        }
                    }
                }
            }
        }

        private static bool IsDefaultValue(object? value, Type type)
        {
            if (value == null) return true;

            if (type.IsValueType)
            {
                var defaultValue = Activator.CreateInstance(type);
                return value.Equals(defaultValue);
            }

            return false;
        }

        private enum SerializationMode
        {
            Serialize,
            Deserialize
        }

        private static PropertyInfo[] GetSerializableProperties(Type type, string? version, SerializationMode mode)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                      .Where(p => p.CanRead && p.CanWrite)
                      .Where(p => !p.GetCustomAttributes<Asn1PropertyAttribute>().Any(a => a.Ignore))
                      .Where(p => IsPropertyValidForVersion(p, version, mode))
                      .OrderBy(p => p.GetCustomAttribute<Asn1PropertyAttribute>()?.Order ?? 0)
                      .ThenBy(p => p.Name)
                      .ToArray();
        }

        private static bool IsPropertyValidForVersion(PropertyInfo prop, string? version, SerializationMode mode)
        {
            var attr = prop.GetCustomAttribute<Asn1PropertyAttribute>();
            if (attr == null) return true;

            if (string.IsNullOrEmpty(version)) return true;

            if (!string.IsNullOrEmpty(attr.SinceVersion))
            {
                if (CompareVersions(version, attr.SinceVersion) < 0)
                {
                    return mode == SerializationMode.Deserialize && attr.Optional;
                }
            }

            if (!string.IsNullOrEmpty(attr.RemovedInVersion))
            {
                if (CompareVersions(version, attr.RemovedInVersion) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static UniversalTagNumber GetDefaultTag(Type type)
        {
            return TypeToTagMap.TryGetValue(type, out var tag) ? tag : UniversalTagNumber.UTF8String;
        }

        /// <summary>
        /// Utility method to serialize any object to ASN.1 format
        /// </summary>
        public static byte[] SerializeToAsn1<T>(T obj, string? targetVersion = null) where T : class
        {
            return Serialize(obj, targetVersion);
        }

        /// <summary>
        /// Utility method to deserialize ASN.1 data and update an existing object's properties
        /// </summary>
        public static void DeserializeFromAsn1<T>(T targetObject, byte[] data, string? expectedVersion = null) where T : class, new()
        {
            var deserializedObject = Deserialize<T>(data, expectedVersion);
            CopyPropertiesFrom(targetObject, deserializedObject);
        }

        /// <summary>
        /// Utility method to copy properties from source object to target object using reflection
        /// </summary>
        public static void CopyPropertiesFrom<T>(T target, object source) where T : class
        {
            var sourceType = source.GetType();
            var targetType = target.GetType();

            foreach (var prop in sourceType.GetProperties())
            {
                var targetProp = targetType.GetProperty(prop.Name);
                if (targetProp?.CanWrite == true)
                {
                    targetProp.SetValue(target, prop.GetValue(source));
                }
            }
        }
    }
}