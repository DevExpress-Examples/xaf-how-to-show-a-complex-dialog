Imports DevExpress.Xpo
Imports DevExpress.ExpressApp
Imports DevExpress.ExpressApp.Actions
Imports DevExpress.Persistent.Base
Imports DevExpress.ExpressApp.Editors
Imports ComplexDialogSample.Module.BusinessObjects

Namespace ComplexDialogSample.Module.Controllers

    <DC.DomainComponent>
    Public Class OrderTemplate

        Public Sub New(ByVal s As Session)
            _Services = New XPCollection(Of Service)(s)
        End Sub

        Public Property DueDate As Date

        Public Property Team As Team

        Private _Services As XPCollection(Of Service)

        Public ReadOnly Property Services As XPCollection(Of Service)
            Get
                Return _Services
            End Get
        End Property
    End Class

    Public Class MyController
        Inherits ViewController

        Public Sub New()
            TargetObjectType = GetType(Office)
            TargetViewType = ViewType.ListView
            Dim action As PopupWindowShowAction = New PopupWindowShowAction(Me, "AssignJobs", PredefinedCategory.RecordEdit)
            action.SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects
            AddHandler action.CustomizePopupWindowParams, New CustomizePopupWindowParamsEventHandler(AddressOf action_CustomizePopupWindowParams)
            AddHandler action.Execute, New PopupWindowShowActionExecuteEventHandler(AddressOf action_Execute)
        End Sub

        Private Sub action_CustomizePopupWindowParams(ByVal sender As Object, ByVal e As CustomizePopupWindowParamsEventArgs)
            Dim os As IObjectSpace = Application.CreateObjectSpace(GetType(Service))
            e.Context = TemplateContext.PopupWindow
            e.View = Application.CreateDetailView(os, New OrderTemplate(CType(os, Xpo.XPObjectSpace).Session))
            CType(e.View, DetailView).ViewEditMode = ViewEditMode.Edit
        End Sub

        Private Sub action_Execute(ByVal sender As Object, ByVal e As PopupWindowShowActionExecuteEventArgs)
            Dim parameters As OrderTemplate = TryCast(e.PopupWindow.View.CurrentObject, OrderTemplate)
            Dim listPropertyEditor As ListPropertyEditor = TryCast(CType(e.PopupWindow.View, DetailView).FindItem("Services"), ListPropertyEditor)
            Dim os As IObjectSpace = Application.CreateObjectSpace(GetType(Team))
            For Each b As Office In e.SelectedObjects
                Dim team As Team = os.GetObject(parameters.Team)
                For Each service As Service In listPropertyEditor.ListView.SelectedObjects
                    Dim order As Order = os.CreateObject(Of Order)()
                    order.DueDate = parameters.DueDate
                    order.Team = team
                    order.Office = os.GetObject(b)
                    order.Service = os.GetObject(service)
                    order.Save()
                Next
            Next

            os.CommitChanges()
        End Sub
    End Class
End Namespace
