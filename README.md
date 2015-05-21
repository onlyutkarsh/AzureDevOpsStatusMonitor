# Introduction
Occasionally when Visual Studio Online (VSO) is down or having some issues, we head over to the [status overview](https://www.visualstudio.com/en-us/support/support-overview-vs.aspx) page to monitor its status. How nice would it be to monitor the status while you code in Visual Studio? I thought it would be useful, and I just developed a tiny extension to monitor the status of VSO from Visual Studio itself.

The extension quitely sits in the Visual Studio status bar displaying an icon - based on whether Visual Studio Online is running smooth or has some issues or completely down. 

![](img/vso_status_inspector.png)

Download the extension from [**VS gallery**](https://visualstudiogallery.msdn.microsoft.com/e87c82b9-dced-4fe2-9a40-f90139c56882)

## Troubleshoot

[Issue: Status icon not visible in the status bar](https://github.com/onlyutkarsh/VSOStatusInspector/issues/1) 

- Go to `Tools` -> `Options`
- Under `Environment` -> `General` category, ensure you have checked `Enable rich client visual experience`
![](img/troubleshoot_options.png)
 


## Facing issues?
Please [raise a issue](https://github.com/onlyutkarsh/VSOStatusInspector/issues/new) and I will try my best to address and fix the issue.

Alternatively you can contribute the fix send the pull request :-)
