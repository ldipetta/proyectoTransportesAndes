
$(document).ready(function () {
    $("#vehiculos").on('change', function () {
        alert("hola");
    });
    
    $('#viajeLibre').on('click', function () {
        if ($(this).is(':checked')) {
            $("#destino").removeClass('hidden');
        } else {
            $('#destino').addClass('hidden');
        }
    });
    $("#Refresh").click(function () {
        initMap();
    });
    var sessionTipo = $("#contenedorCabecera").data('value');
    var sessionValue = $("#menuComun").data('value');

    if (sessionTipo === "Cliente" && sessionValue === "no") {
        //$("#menuComun").append("<li><a asp-area='' asp-controller='Viaje' asp-action='Index'>Solicitar traslado</a></li>");

    }
    else if (sessionTipo === "Cliente" && sessionValue === "si") {
        $("#bienvenida").addClass("hidden");
        $('#solicitarViaje').removeClass('hidden');
        $('#misViajes').removeClass('hidden');
    }
    else {
        //$("#contenedorCabecera").append("<ul class='nav navbar-nav'><li><a asp-area='' asp-controller='Vehiculo' asp-action='Index'>Vehiculos</a></li></ul>");
        $("#vehiculos").removeClass("hidden");
        $("#empleados").removeClass("hidden");
        $("#choferes").removeClass("hidden");
        $("#solicitarViaje").addClass("hidden");
        $("#about").addClass("hidden");
        $("#contact").addClass("hidden");
        $("#ingresarViaje").removeClass("hidden");
        $("#clientes").removeClass("hidden");
        $("#bienvenida").addClass("hidden");
        $("#misViajes").addClass("hidden");
    }
    var userName = $("#userName").data('value');
    if (userName !== "") {
        $("#salir").removeClass("hidden");
        $("#userName").append("<a>Hola " + userName + "</a>");
    }
    else {


    }

});
