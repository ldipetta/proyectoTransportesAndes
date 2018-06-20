
$(document).ready(function () {
    var sessionValue = $("#contenedorCabecera").data('value');
    if (sessionValue === "Cliente") {
        //$("#menuComun").append("<li><a asp-area='' asp-controller='Viaje' asp-action='Index'>Solicitar traslado</a></li>");
        
    }
    else
    {
        //$("#contenedorCabecera").append("<ul class='nav navbar-nav'><li><a asp-area='' asp-controller='Vehiculo' asp-action='Index'>Vehiculos</a></li></ul>");
        $("#vehiculos").removeClass("hidden");
        $("#empleados").removeClass("hidden");
        $("#solicitarViaje").addClass("hidden");
        $("#about").addClass("hidden");
        $("#contact").addClass("hidden");
    }
    var userName = $("#userName").data('value');
    if (userName !== "") {
        $("#salir").removeClass("hidden");
        $("#userName").append("<a>Hola " + userName + "</a>");
    }
    else
    {

       
    }
});
