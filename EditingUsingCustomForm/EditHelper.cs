// Copyright 2010 ESRI
// 
// All rights reserved under the copyright laws of the United States
// and applicable international laws, treaties, and conventions.
// 
// You may freely redistribute and use this sample code, with or
// without modification, provided you include the original copyright
// notice and use restrictions.
// 
// See the use restrictions at &lt;your ArcGIS install location&gt;/DeveloperKit10.0/userestrictions.txt.
// 

using System;
using System.Collections.Generic;
using System.Text;

namespace EditingUsingCustomForm
{
    class EditHelper
    {
        private static EditHelper instance;
        private MainForm m_mainform = null;
        private bool m_editorFormOpen;

        //private constructor - external classes cannot create a 'new' EditHelper instance
        private EditHelper()
        {
        }

        public static MainForm TheMainForm
        {
            get
            {
                if (instance != null)
                {
                    return instance.m_mainform;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (instance == null)
                {
                    instance = new EditHelper();
                }

                instance.m_mainform = value;

            }
        }

        public static bool IsEditorFormOpen
        {
            get
            {
                if (instance != null)
                {
                    return instance.m_editorFormOpen;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                if (instance == null)
                {
                    instance = new EditHelper();
                }

                instance.m_editorFormOpen = value;

            }
        }




    }
}
