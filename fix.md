
1. public assemblyinfo.cs增加
``` cs
  [assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)] 
```
2. MSBuid中的PostSharp15类名需改为PostSharp，
   调试参数 ：
   - msbuild:  C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe

   - ExecutePostSharp.proj "/p:In=PostSharpTest.exe" "/p:Out=test.exe" "/p:ReferenceDirectory=D:\projects\Aop\source\PostSharp.MSBuildx\bin\Debug" /fl /flp:v=diag

3. 需要在执行目录中增加 文件:
    PostSharp-Platform.config
    PostSharp-Library.config
    PostSharp.Core.XmlSerializers.dll
4. ConfigurationHelper.cs 70行改动，暂时不制定publictoken 强命名程序集
5. 增加反编译项目 PostSharp.Core.XmlSerializers，因为此项目引用到Core项目，而core项目的configurationhelper又引用到此项目会造成循环引用故将此项目直接放到Core项目中
6. 获取 {$ResolvedReferences} 属性失败

