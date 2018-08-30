function AzCreateResourceGroupForRegionAndPackage($region, $shortname)
{
 $env = $global:inputObject.envName
 $x = AzCreateResourceName "rg" $env $shortname $region
 return $x
}

function AzCreateGlobalAppInsightsNameForPackage($shortname)
{
    $env = $global:inputObject.envName
	
    $r = AzCreateResourceName "ai" $env $shortname "glb"
	return $r
}

Export-ModuleMember -Function "AzCreateResourceGroupForRegionAndPackage"
Export-ModuleMember -Function "AzCreateGlobalAppInsightsNameForPackage"