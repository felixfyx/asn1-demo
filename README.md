# ASN.1 Demo Project
Here, we want to be able to make a way to to save, load,
encode and decode ASN.1 file.

## Context
Originally, this was intended for a project that uses ASN.1 format files
to read settings. 

However, due to how in-flexible the format is, some other format was considered
more strongly.

## How to build and run
Just type `dotnet run` in the terminal.

## Experiement results
All sample code regarding each Agency is in `Agency.cs` file and `Program.cs`.

### Results
- Basic saving and loading &rarr; OK
- Using old class with 5 variables in a class with 6 variables 
(extra variable at the end) &rarr; OK
- Same as previous test but different order of variables 
(extra variable in the middle) &rarr; OK
- Rearranged variables &rarr; NOT OK (Most variables end up with defaults)  

## Note
For data that did not match, it will use a default value. 

## Conclusion
Might want to hold off using ASN.1 unless we really need to due to its
inflexible nature