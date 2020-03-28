# This is an extension for .NET CSharp Kernel for Jupyter Notebook
The estension available in this assembly allow to import .NTE notebook as module
The directive is #!import [path to notebook]  

For example:
**#!import lib\mynotebook**


## How to use in a notebook
    [In] 
    #r "nuget:Chino.Jupyter.Extensions, 1.0.4-beta"
    [out] 
    Installed package Chino.Jupyter.Extensions version 1.0.4-beta
    ImportExtension is loaded. It adds the import notebook as module. 
    Try it by running: #!import path\notebook name 
    
    [in] 
    #!import lib\class_definition
    #!import lib\class_nested_module
    [out]
    Loading notebook lib\class_definition ...
    Notebook lib\class_definition loaded
    Loading notebook lib\class_nested_module ...
    Loading notebook lib\class_mamal ...
    Notebook lib\class_mamal loaded
    Notebook lib\class_nested_module loaded
    
    [in]
    var me = new Person(){Name="Roberto"};
    #!who
    [out]
    [me, mydog]
    
    [in]
    display(mydog)
    
    [out]
    PetName
    Bob

Notebooks are organized as following:\
ROOT\
│ main.ipynb\
│\
└──lib\
  class_definition.ipynb\
  class_mamal.ipynb\
  class_nested_module.ipynb
