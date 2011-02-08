<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="CharacterConverter.aspx.cs" Inherits="CharacterConverter.Web.CharacterConverterView" Async="True" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Character converter Web</title>
</head>
<body>
    <form id="form1" runat="server" oninit="FormInit">
	 <asp:ScriptManager ID="_scripts" runat="server"/>
    <asp:UpdatePanel ID="_panel" runat="server" ChildrenAsTriggers="False" 
		 UpdateMode="Conditional">
		 <ContentTemplate>
			 <table>
				 <tr>
					 <td>Host:</td>
					 <td><asp:TextBox ID="_tbHost" runat="server"/></td>
					 <td rowspan ="6" align="left" valign ="top"><asp:Label ID="_lbInfo" runat="server" Width="100%"/></td>
				 </tr>
				 <tr>
					 <td>Port:</td>
					 <td><asp:TextBox ID="_tbPort" runat="server"/></td>
				 </tr>
				 <tr>
					 <td>Database:</td>
					 <td><asp:TextBox ID="_tbBase" runat="server"/></td>
				 </tr>
				 <tr>
					 <td>Login:</td>
					 <td><asp:TextBox ID="_tbUser" runat="server"/></td>
				 </tr>
				 <tr>
					 <td>Password:</td>
					 <td><asp:TextBox ID="_tbPass" runat="server"/></td>
				 </tr>
				 <tr>
					 <td colspan="2">
						 <asp:Button ID="_btnConvert" runat="server" onclick="BtnConvert_Click" Text="Go!" 
							 Width="100%" />
					 </td>
				 </tr>
			 </table>
		 </ContentTemplate>
	 </asp:UpdatePanel>
    </form>
</body>
</html>
