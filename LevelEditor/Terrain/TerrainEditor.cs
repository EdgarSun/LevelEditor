﻿//Copyright © 2014 Sony Computer Entertainment America LLC. See License.txt.

using System;
using System.ComponentModel.Composition;

using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

using LevelEditorCore;

namespace LevelEditor.Terrain
{

    [Export(typeof(TerrainEditor))]    
    [Export(typeof(IInitializable))]    
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class TerrainEditor : IInitializable
    {        
        /// <summary>
        /// Gets TerrainEditorControl</summary>
        public TerrainEditorControl TerrainEditorControl
        {
            get { return m_control; }
        }
       
        #region IInitializable Members

        void IInitializable.Initialize()
        {          
            m_control = new TerrainEditorControl();
            ControlInfo cinfo = new ControlInfo("Terrain Editor", "Edit terrain properties", StandardControlGroup.Right);
            m_controlHostService.RegisterControl(m_control, cinfo, null);
            m_contextRegistry.ActiveContextChanged += ContextRegistry_ActiveContextChanged;
        }

        private void ContextRegistry_ActiveContextChanged(object sender, EventArgs e)
        {
            IGameContext game = m_contextRegistry.GetActiveContext<IGameContext>();
            var  observableContext = game.As<IObservableContext>();
            if (m_observableContext == observableContext) return;
            if (m_observableContext != null)
            {
                m_observableContext.ItemInserted -= m_observableContext_ItemInserted;
                m_observableContext.ItemRemoved -= m_observableContext_ItemRemoved;
                m_observableContext.ItemChanged -= m_observableContext_ItemChanged;
            }
            m_observableContext = observableContext;           
            m_control.GameContext = game;
            
            if (m_observableContext != null)
            {
                m_observableContext.ItemInserted += m_observableContext_ItemInserted;
                m_observableContext.ItemRemoved += m_observableContext_ItemRemoved;
                m_observableContext.ItemChanged += m_observableContext_ItemChanged;
            }
            m_control.PopulatedTerrainCmbox();
        }

        void m_observableContext_ItemChanged(object sender, ItemChangedEventArgs<object> e)
        {
            UpdateTerrainControl(e.Item);            
        }

        void m_observableContext_ItemRemoved(object sender, ItemRemovedEventArgs<object> e)
        {
            UpdateTerrainControl(e.Item);            
        }

        void m_observableContext_ItemInserted(object sender, ItemInsertedEventArgs<object> e)
        {
            UpdateTerrainControl(e.Item);            
        }
        
        #endregion


        private void UpdateTerrainControl(object item)
        {
            if (item.Is<TerrainGob>())
                m_control.PopulatedTerrainCmbox();
            else if (IsTerrainChild(item))
                m_control.ReBind();

        }
        private bool IsTerrainChild(object item)
        {
            DomNode node = item.As<DomNode>();
            if (node != null)
            {               
                foreach (ChildInfo chInfo in Schema.terrainGobType.Type.Children)
                {
                    if (chInfo.Type.Equals(node.Type))
                        return true;
                }
            }
            return false;                       
        }
        private TerrainEditorControl m_control;

        [Import(AllowDefault = false)]
        private IContextRegistry m_contextRegistry;

        [Import(AllowDefault = false)]
        private IControlHostService m_controlHostService;

        private IObservableContext m_observableContext;        
    }
}
