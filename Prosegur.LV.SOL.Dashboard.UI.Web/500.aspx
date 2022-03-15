<%@ Page Language="C#" %>

<%  
    bool _isAjaxRequest = false;

    if (Request["isAjax"] != null)
    {
        _isAjaxRequest = true;
    }
    var page = HttpContext.Current.Handler as Page;
    
    if (Request != null)
    {
        _isAjaxRequest = (Request["X-Requested-With"] == "XMLHttpRequest") || ((Request.Headers != null) && (Request.Headers["X-Requested-With"] == "XMLHttpRequest"));
    }
    
    if (Response.StatusCode != 500) { Response.StatusCode = 500; } 
    
%>

<% if (!_isAjaxRequest) { %>
    <!DOCTYPE html>
    <html xmlns="http://www.w3.org/1999/xhtml">
    <head>

        <link href="/Estadisticas/Content/bootstrap-4.0.0-beta.2.css" rel="stylesheet"/>
        <meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
        <title>500 - DTI - Dashboard</title>

        <style>

            .error-template {padding: 40px 15px;}

        </style>
    </head>
    <body>
        <div class="container">

            <div class="error-template">
                <div class="col-md-10">
                    <div class="alert alert-dark" role="alert">
                        <h4 class="alert-heading">500 Error!</h4>
                        <p><%= Response.StatusDescription %></p>
                    </div>
                </div>
            </div>
            
        </div>
    </body>
    </html>
<% } else { %>
        <%= Response.StatusDescription %>
<% } %>
