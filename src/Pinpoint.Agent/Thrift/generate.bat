thrift-0.9.2.exe -gen csharp Command.thrift
thrift-0.9.2.exe -gen csharp Pinpoint.thrift
thrift-0.9.2.exe -gen csharp Trace.thrift
xcopy /y gen-csharp\Pinpoint\Agent\Thrift\Dto\* Dto
rd /s /q gen-csharp