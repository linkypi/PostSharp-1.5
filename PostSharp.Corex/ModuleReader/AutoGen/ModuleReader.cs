using PostSharp.CodeModel;
using PostSharp.PlatformAbstraction;

namespace PostSharp.ModuleReader
{
    internal sealed partial class ModuleReader
    {

        private ExportedTypeDeclaration[] CreateArrayOfExportedTypeDeclaration(MetadataTableOrdinal table)
        {
            ExportedTypeDeclaration[] objects = new ExportedTypeDeclaration[this.tables.Tables[(int)table].RowCount];
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i] = new ExportedTypeDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }

        private AssemblyRefDeclaration[] CreateArrayOfAssemblyRefDeclaration( MetadataTableOrdinal table )
        {
            AssemblyRefDeclaration[] objects = new AssemblyRefDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new AssemblyRefDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }


        private ManifestFileDeclaration[] CreateArrayOfManifestFileDeclaration( MetadataTableOrdinal table )
        {
            ManifestFileDeclaration[] objects = new ManifestFileDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new ManifestFileDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }


        private ModuleRefDeclaration[] CreateArrayOfModuleRefDeclaration( MetadataTableOrdinal table )
        {
            ModuleRefDeclaration[] objects = new ModuleRefDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new ModuleRefDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }


        private TypeDefDeclaration[] CreateArrayOfTypeDefDeclaration( MetadataTableOrdinal table )
        {
            TypeDefDeclaration[] objects = new TypeDefDeclaration[this.tables.Tables[(int) table].RowCount];
            /*
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new TypeDefDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
             */
            return objects;
        }


        private TypeRefDeclaration[] CreateArrayOfTypeRefDeclaration( MetadataTableOrdinal table )
        {
            TypeRefDeclaration[] objects = new TypeRefDeclaration[this.tables.Tables[(int) table].RowCount];
            /*
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new TypeRefDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
             */
            return objects;
        }


        private TypeSpecDeclaration[] CreateArrayOfTypeSpecDeclaration( MetadataTableOrdinal table )
        {
            TypeSpecDeclaration[] objects = new TypeSpecDeclaration[this.tables.Tables[(int) table].RowCount];
            /*
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new TypeSpecDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
             */
            return objects;
        }


        private MethodDefDeclaration[] CreateArrayOfMethodDefDeclaration( MetadataTableOrdinal table )
        {
            MethodDefDeclaration[] objects = new MethodDefDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new MethodDefDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }


        private FieldDefDeclaration[] CreateArrayOfFieldDefDeclaration( MetadataTableOrdinal table )
        {
            FieldDefDeclaration[] objects = new FieldDefDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new FieldDefDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }


        private EventDeclaration[] CreateArrayOfEventDeclaration( MetadataTableOrdinal table )
        {
            EventDeclaration[] objects = new EventDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new EventDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }


        private PropertyDeclaration[] CreateArrayOfPropertyDeclaration( MetadataTableOrdinal table )
        {
            PropertyDeclaration[] objects = new PropertyDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new PropertyDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }


        private PermissionSetDeclaration[] CreateArrayOfPermissionSetDeclaration( MetadataTableOrdinal table )
        {
            PermissionSetDeclaration[] objects = new PermissionSetDeclaration[this.tables.Tables[(int) table].RowCount];

            /*
            if (!this.currentlyLazyLoading)
            {
                for ( int i = 0; i < objects.Length; i++ )
                {
                    objects[i] = new PermissionSetDeclaration {MetadataToken = new MetadataToken( table, i )};
                }
            }*/
            return objects;
        }


        private MethodSpecDeclaration[] CreateArrayOfMethodSpecDeclaration( MetadataTableOrdinal table )
        {
            MethodSpecDeclaration[] objects = new MethodSpecDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new MethodSpecDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }


        private GenericParameterDeclaration[] CreateArrayOfGenericParameterDeclaration( MetadataTableOrdinal table )
        {
            GenericParameterDeclaration[] objects =
                new GenericParameterDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new GenericParameterDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }


        private ManifestResourceDeclaration[] CreateArrayOfManifestResourceDeclaration( MetadataTableOrdinal table )
        {
            ManifestResourceDeclaration[] objects =
                new ManifestResourceDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new ManifestResourceDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }


        private StandaloneSignatureDeclaration[] CreateArrayOfStandaloneSignatureDeclaration( MetadataTableOrdinal table )
        {
            StandaloneSignatureDeclaration[] objects =
                new StandaloneSignatureDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new StandaloneSignatureDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }

        private ParameterDeclaration[] CreateArrayOfParameterDeclaration( MetadataTableOrdinal table )
        {
            ParameterDeclaration[] objects = new ParameterDeclaration[this.tables.Tables[(int) table].RowCount];
            for ( int i = 0 ; i < objects.Length ; i++ )
            {
                objects[i] = new ParameterDeclaration {MetadataToken = new MetadataToken( table, i )};
            }
            return objects;
        }

        internal void ReadModule()
        {
            this.ReadModule(Platform.Current.ReadModuleStrategy);
        }
    }
}