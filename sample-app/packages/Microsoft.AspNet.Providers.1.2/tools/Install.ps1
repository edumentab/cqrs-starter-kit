param($installPath, $toolsPath, $package, $project)
try {
    # Set up variables
    $timestamp = (Get-Date).ToString('yyyyMMddHHmmss')
    $projectName = [IO.Path]::GetFileName($project.ProjectName.Trim([IO.PATH]::DirectorySeparatorChar, [IO.PATH]::AltDirectorySeparatorChar))
    $catalogName = "aspnet-$projectName-$timestamp"
    $connectionString ="Data Source=.\SQLEXPRESS;Initial Catalog=$catalogName;Integrated Security=SSPI"
    $connectionStringToken = "Data Source=.\SQLEXPRESS;Initial Catalog=aspnet-$projectName"
    $config = $project.ProjectItems | Where-Object { $_.Name -eq "Web.config" }    
    $configPath = ($config.Properties | Where-Object { $_.Name -eq "FullPath" }).Value
    
    # Load the Config File
    $xml = New-Object System.Xml.XmlDocument
    $xml.Load($configPath)
    
    function CommentNode($node) {
        if (!$node) {
            return;
        }
        
        $commentNode = $xml.CreateComment($node.OuterXml)
        $parent = $node.ParentNode
        $parent.InsertBefore($commentNode, $node) | Out-Null
        $parent.RemoveChild($node) | Out-Null
    }
    
    function GetConnectionStringToken($connectionStringValue, $tokenName) {
        $startIndex = $connectionStringValue.IndexOf($tokenName, [StringComparison]::OrdinalIgnoreCase)
        if ($startIndex -ge 0) {
            $endIndex = $connectionStringValue.IndexOf(';', $startIndex + 1)
            $endIndex = if ($endIndex -lt 0) { $connectionStringValue.Length } else { $endIndex }
            
            ';' + $connectionStringValue.Substring($startIndex, $endIndex - $startIndex)
        }
    }
    
    
    # Comment out older providers
    $node = $xml.SelectSingleNode("/configuration/system.web/membership/providers/add[@type='System.Web.Security.SqlMembershipProvider']")
    $addedNode = $xml.SelectSingleNode("/configuration/system.web/membership/providers/add[@name='DefaultMembershipProvider']")
    
    if ($node) {
        # Copy all attributes other than 'name', 'type' and 'connectionStringName' to the newly added node.
        $node.Attributes | Where { ! (@('name', 'type', 'connectionStringName') -contains $_.name) } | ForEach {
            $addedNode.SetAttribute($_.name, $_.value)
        }
    }
    
    $oldConnectionNode = $xml.SelectSingleNode("/configuration/connectionStrings/add[@name='$($node.connectionStringName)']")
    if ($oldConnectionNode) {
        $oldConnectionValue = $oldConnectionNode.connectionString
        # Copy AttachDBFileName and UserInstance values from the old connection string if they exist
        if ($oldConnectionValue.IndexOf('AttachDBFilename', [StringComparison]::OrdinalIgnoreCase) -ge 0) {
            $connectionString += ";AttachDBFilename=|DataDirectory|$catalogName.mdf"
        }
        $connectionString += (GetConnectionStringToken $oldConnectionValue 'User Instance')
    }
    
    CommentNode $node
    CommentNode $oldConnectionNode
    
    $node = $xml.SelectSingleNode("/configuration/system.web/profile/providers/add[@type='System.Web.Profile.SqlProfileProvider']")
    $oldConnectionNode = $xml.SelectSingleNode("/configuration/connectionStrings/add[@name='$($node.connectionStringName)']")
    CommentNode $node
    CommentNode $oldConnectionNode
    
    $node = $xml.SelectSingleNode("/configuration/system.web/roleManager/providers/add[@type='System.Web.Security.SqlRoleProvider']")
    $oldConnectionNode = $xml.SelectSingleNode("/configuration/connectionStrings/add[@name='$($node.connectionStringName)']")
    $connectionStringsToComment += $node.connectionStringName
    CommentNode $node
    CommentNode $oldConnectionNode
    
    # Change the Connection string
    $connectionStrings = $xml.SelectSingleNode("/configuration/connectionStrings")
    if (!$connectionStrings) {
        $connectionStrings = $xml.CreateElement("connectionStrings")
        $xml.configuration.AppendChild($connectionStrings) | Out-Null
    }
    
    if (!($connectionStrings.SelectNodes("add[@name='DefaultConnection']") | Where { $_.connectionString.StartsWith($connectionStringToken, 'OrdinalIgnoreCase') })) {
        # If there aren't any connection strings that look like ours, proceed to add one
        
        $newConnectionNode = $xml.CreateElement("add")
        $newConnectionNode.SetAttribute("name", "DefaultConnection")
        $newConnectionNode.SetAttribute("providerName", "System.Data.SqlClient")
        $newConnectionNode.SetAttribute("connectionString", $connectionString)
        
        $connectionStrings.AppendChild($newConnectionNode) | Out-Null
    }
    
    # Save the Config File 
    $xml.Save($configPath)
    
} catch {
    Write-Error "Unable to update the web.config file at $configPath. Add the following connection string to your config: <add name=`"DefaultConnection`" providerName=`"System.Data.SqlClient`" connectionString=`"$connectionString`" />"
}
