This project was made for the JetBrains internship program in 2023.
The goal of this tool is to analyze a unity scene dump, determine scene hierarchy and find which script are unused. 
For achieving this, I am using a yaml parser nuget to extract information from “.unity” files and I determine scene hierarchy and also which scripts are used for every single file. 
Afterwards, I parse all scripts using Roslyn and determine which ones are serialized. Finally, I write the information in a csv file. 
