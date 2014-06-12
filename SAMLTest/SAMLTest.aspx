<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SAMLTest.aspx.cs" Inherits="SAMLTest.WebForm1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body style="height: 900px">
    <form id="form1" runat="server">
    <div>
        <div style="width: 1019px; height: 300px;">
            <asp:Label ID="Label1" runat="server" Text="SAML Response"></asp:Label><hr />
            <asp:TextBox ID="TextBox1" runat="server" Height="215px" Width="1000px" TextMode="MultiLine"></asp:TextBox>
        </div>           
        <div style="width: 1019px; height: 300px;">
            <asp:Label ID="Label2" runat="server" Text="Processing Information"></asp:Label><hr />
            <asp:TextBox ID="TextBox2" runat="server" Height="215px" Width="1000px" TextMode="MultiLine"></asp:TextBox>
        </div>
        <div style="width: 1019px; height: 300px;">

        </div>
    </div>
    </form>
</body>
</html>
