<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
	xmlns:i="urn:indexManager">
	<xsl:output method="xml" version="1.0" encoding="UTF-8" indent="yes"/>
	<?index name="test" nodes="/files/file/root/element" key="@name" ?>
	<xsl:template match="* | @*">
		<xsl:copy>
			<xsl:apply-templates select="* | @*"/>
		</xsl:copy>
	</xsl:template>
	<xsl:template match="ref">
		[<xsl:value-of select="i:getIndexedNode( 'test', @name )/@displayName"/>]
	</xsl:template>
</xsl:stylesheet>
