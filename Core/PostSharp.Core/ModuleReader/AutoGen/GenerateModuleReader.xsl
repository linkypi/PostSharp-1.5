<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format" xmlns:xs="http://www.w3.org/2001/XMLSchema" xmlns:fn="http://www.w3.org/2005/xpath-functions" xmlns:xdt="http://www.w3.org/2005/xpath-datatypes">

<xsl:output method="text"/>

	<xsl:template match="/">
	
	using System;
	using PostSharp.CodeModel;
	
	namespace PostSharp.ModuleReader
	{
		 internal sealed partial class ModuleReader : IDisposable
		 {
		 			 <xsl:apply-templates />
		 }
	
	}
	
	</xsl:template>

	<xsl:template match="type">
		
			private <xsl:value-of select="."/>[] CreateArrayOf<xsl:value-of select="."/>(  MetadataTableOrdinal table )
			{
				<xsl:value-of select="."/>[] objects = new <xsl:value-of select="."/>[this.tables.Tables[(int)table].RowCount];
				for (int i = 0; i &lt; objects.Length; i++)
				{
					objects[i] = new <xsl:value-of select="."/>();
					objects[i].MetadataToken = new MetadataToken(table, i);
				}
				return objects;
			
			}
		
	
	</xsl:template>
</xsl:stylesheet>
