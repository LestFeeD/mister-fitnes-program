﻿#pragma checksum "..\..\ListExer.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "56981A110DCC3D45C00A9B49A3632D5EE1F48CB149D3CF63E8ACAE64A4A1195B"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using FitnesPlan;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace FitnesPlan {
    
    
    /// <summary>
    /// ListExer
    /// </summary>
    public partial class ListExer : System.Windows.Window, System.Windows.Markup.IComponentConnector, System.Windows.Markup.IStyleConnector {
        
        
        #line 26 "..\..\ListExer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button TransitionBut;
        
        #line default
        #line hidden
        
        
        #line 69 "..\..\ListExer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ScrollViewer Scrow;
        
        #line default
        #line hidden
        
        
        #line 97 "..\..\ListExer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox Filter;
        
        #line default
        #line hidden
        
        
        #line 101 "..\..\ListExer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock watermarkUsername;
        
        #line default
        #line hidden
        
        
        #line 124 "..\..\ListExer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ItemsControl AllExer;
        
        #line default
        #line hidden
        
        
        #line 145 "..\..\ListExer.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ItemsControl ExerPlanNov;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/FitnesPlan;component/listexer.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\ListExer.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.TransitionBut = ((System.Windows.Controls.Button)(target));
            
            #line 26 "..\..\ListExer.xaml"
            this.TransitionBut.Click += new System.Windows.RoutedEventHandler(this.TransitionBut_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Scrow = ((System.Windows.Controls.ScrollViewer)(target));
            return;
            case 3:
            this.Filter = ((System.Windows.Controls.TextBox)(target));
            
            #line 98 "..\..\ListExer.xaml"
            this.Filter.TextChanged += new System.Windows.Controls.TextChangedEventHandler(this.Filter_TextChanged);
            
            #line default
            #line hidden
            
            #line 99 "..\..\ListExer.xaml"
            this.Filter.GotFocus += new System.Windows.RoutedEventHandler(this.txtUsername_GotFocus);
            
            #line default
            #line hidden
            
            #line 100 "..\..\ListExer.xaml"
            this.Filter.LostFocus += new System.Windows.RoutedEventHandler(this.txtUsername_LostFocus);
            
            #line default
            #line hidden
            return;
            case 4:
            this.watermarkUsername = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.AllExer = ((System.Windows.Controls.ItemsControl)(target));
            return;
            case 7:
            this.ExerPlanNov = ((System.Windows.Controls.ItemsControl)(target));
            return;
            }
            this._contentLoaded = true;
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        void System.Windows.Markup.IStyleConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 6:
            
            #line 132 "..\..\ListExer.xaml"
            ((System.Windows.Controls.Border)(target)).MouseUp += new System.Windows.Input.MouseButtonEventHandler(this.Border_MouseUp);
            
            #line default
            #line hidden
            break;
            case 8:
            
            #line 165 "..\..\ListExer.xaml"
            ((System.Windows.Controls.Border)(target)).MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.SpecExer_MouseDown);
            
            #line default
            #line hidden
            break;
            }
        }
    }
}

