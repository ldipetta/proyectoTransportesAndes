
$(document).ready(function () {
    $("#btnFlete").click(function () {
        $("#contenedorFlete").removeClass("hidden");
        $("#contenedorMudanza").addClass("hidden");
        $("#mapa").removeClass("hidden");

        $("#btnFlete").removeClass("btnPersonalizadoCuentaNueva");
        $("#btnFlete").addClass("btnPersonalizadoIniciarSesion");
        $("#btnMudanza").removeClass("btnPersonalizadoIniciarSesion");
        $("#btnMudanza").addClass("btnPersonalizadoCuentaNueva");
    });

    $("#btnMudanza").click(function () {
        $("#contenedorFlete").addClass("hidden");
        $("#contenedorMudanza").removeClass("hidden");
        $("#mapa").removeClass("hidden");
        $("#btnMudanza").removeClass("btnPersonalizadoCuentaNueva");
        $("#btnMudanza").addClass("btnPersonalizadoIniciarSesion");
        $("#btnFlete").removeClass("btnPersonalizadoIniciarSesion");
        $("#btnFlete").addClass("btnPersonalizadoCuentaNueva");
    });
    
    $('#sinDestino').on('click', function () {
        if ($(this).is(':checked')) {
            $("#contenedorDestino").removeClass('hidden');
        } else {
            $('#contenedorDestino').addClass('hidden');
        }
    });
    $('#utilizarDireccionCliente').on('click', function () {
        if ($(this).is(':checked')) {
            $("#contenedorDestino").addClass('hidden');
        } else {
            $('#contenedorDestino').removeClass('hidden');
        }
    });
    $('#viajeCompartido').on('click', function () {
        if ($(this).is(':checked')) {
            $("#direcciones").addClass('hidden');
            initMap(true);
        } else {
            $('#direcciones').removeClass('hidden');
            initMap(false);
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
        $('#tarifas').removeClass("hidden");
        $('#presupuestos').removeClass("hidden");
        $('#liquidarViajes').removeClass("hidden");
        $('#estadisticas').removeClass("hidden");

    }
    var userName = $("#userName").data('value');
    if (userName !== "") {
        $("#salir").removeClass("hidden");
        $("#userName").append("<a>Hola " + userName + "</a>");
    }
   

    

});
