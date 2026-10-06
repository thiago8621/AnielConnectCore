var mapa;
var options;
var infowindow;
var latlng;
var geocoder;
var newLocation = null;
var markers = [{
    marker: null,
    info: null
}];
var idClasse = 0;

function loadMap(divMapa, latitude, longitude) {
    setOptions(latitude, longitude);
    mapa = new google.maps.Map(document.getElementById(divMapa), options);
}

function setOptions(latitude, longitude) {
    options = {
        zoom: 16,
        center: getLatLng(latitude, longitude),
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };
}

function getLatLng(latitude, longitude) {
    return new google.maps.LatLng(latitude, longitude, true);
}

function setMarker(titulo, posicao, isDraggable, conteudo, icone) {
    var marker = new google.maps.Marker({
        position: posicao,
        map: mapa,
        title: titulo,
        draggable: isDraggable,
        animation: google.maps.Animation.DROP
    });

    if (icone !== "") {
        marker.setIcon(icone);
    }

    var info = addInfo(conteudo);

    addMarker(marker, info);

    //markers.push(marker);

    addListener(markers[markers.length - 1], 'click');
}

function addMarker(mark, info) {
    var vmark = {
        marker: mark,
        info: info
    };

    markers.push(vmark);

}

function getCoordenadas() {
    var lat = markers[1].marker.getPosition().lat();
    var lng = markers[1].marker.getPosition().lng();

    //   $(function() {
    if ($('#txtLatitude', top.document).length === 1) {
        $('#txtLatitude', top.document).attr('value', lat);
        $('#txtLongitude', top.document).attr('value', lng);
    }
    else if ($('#gridServicos', top.document).length === 1) {
        PageMethods.Georeferenciar(idClasse, lat, lng);
    }
    else if ($('#txtLatitude', $('#frameAddEndereco', top.document).contents()).length === 1) {
        $('#txtLatitude', $('#frameAddEndereco', top.document).contents()).attr('value', lat);
        $('#txtLongitude', $('#frameAddEndereco', top.document).contents()).attr('value', lng);
    }


    // }
    // );
}

function getCoordenadasAuto() {
    var lat = markers[1].marker.getPosition().lat();
    var lng = markers[1].marker.getPosition().lng();

    PageMethods.Georeferenciar(idClasse, lat, lng,

        function (result) {
            window.opener = window;
            window.close("#");
        });

}


function addListener(element, action) {
    google.maps.event.addListener(element.marker, action, function () {

        if (geocoder !== "") {
            getCoordenadas();
        }

        element.info.open(mapa, element.marker);
    });
}

function addInfo(html) {
    var contentString = html;

    var infowindow = new google.maps.InfoWindow({
        content: contentString
    });

    return infowindow;
}

function geocodificarEndereco(divMapa, endereco) {

    loadMap(divMapa, 0, 0);

    geocoder = new google.maps.Geocoder();

    geocoder.geocode(
        { 'address': endereco },
        function (results, status) {
            if (status === google.maps.GeocoderStatus.OK) {

                setMarker(endereco, results[0].geometry.location, true, builderConteudo("Endereço", endereco), "");

                mapa.setCenter(results[0].geometry.location);
            }
            else {
                alert("Ocorreu um erro ao carregar georeferência do endereço.\nFavor verifique o endereço e tente novamente.\nEndereço: " + endereco);
            }
        });

}

function builderConteudo(title, value) {
    return '<div id="content">' +
        '<div id="siteNotice">' +
        '</div>' +
        '<h3 id="firstHeading" class="firstHeading">' + title + '</h1><hr/>' +
        '<div id="bodyContent">' +
        value +
        '</div>' +
        '</div>';
}

function DesenharRota(pontosRota, MenorTrajeto, TrajetosAlternativos, EvitarRodovias, EvitarPedagios) {
    var directionsService = new google.maps.DirectionsService();

    var rendererOptions = {
        map: mapa,
        suppressMarkers: true,
        suppressInfoWindows: true
    };

    var directionsDisplay = new google.maps.DirectionsRenderer(rendererOptions);

    var pontoInicio = getLatLng(pontosRota[0].Latitude, pontosRota[0].Longitude);

    var pontoFinal = getLatLng(pontosRota[pontosRota.length - 1].Latitude, pontosRota[pontosRota.length - 1].Longitude);

    var pontosPassagem = [];

    for (var i = 1; i < pontosRota.length - 1; i++) {
        pontosPassagem.push({
            location: getLatLng(pontosRota[i].Latitude, pontosRota[i].Longitude),
            stopover: true
        });
    }

    var requisicao = {
        origin: pontoInicio,
        destination: pontoFinal,
        waypoints: pontosPassagem,
        optimizeWaypoints: MenorTrajeto,
        travelMode: google.maps.DirectionsTravelMode.DRIVING,
        provideRouteAlternatives: TrajetosAlternativos,
        avoidHighways: EvitarRodovias,
        avoidTolls: EvitarPedagios
    }

    var distanciaRota = 0;

    directionsService.route(requisicao,
        function (response, status) {
            if (status === google.maps.DirectionsStatus.OK) {
                directionsDisplay.setDirections(response);
                var rota = response.routes[0];

                for (var i = 0; i < rota.legs.length; i++) {
                    distanciaRota = distanciaRota + rota.legs[i].distance.value;
                }

                distanciaRota = Math.round(distanciaRota / 1000);

                showAlerta('<p>Kilometragem necessária: ' + distanciaRota + ' Km</p>');
            }
            else {

                if (automatico === "S") {
                    window.opener = window;
                    window.close("#");
                }
                else {
                    alert("Não foi possivel visualizar a rota.");
                }
            }
        }
    );

    for (var y = 0; y < pontosRota.length; y++) {
        setMarker('', getLatLng(pontosRota[y].Latitude, pontosRota[y].Longitude), false, builderConteudo(pontosRota[y].NumOS, pontosRota[y].Hora + '<br/>' + pontosRota[y].Descricao), pontosRota[y].Url);
    }
}

function renderDirections(result) {
    var rendererOptions = {
        map: mapa,
        suppressMarkers: true,
        suppressInfoWindows: true
    };

    var directionsRender = new google.maps.DirectionRender(rendererOptions);

    directionsRender.setDirections(result);

    //distanciaRota = distanciaRota + (distancia / 1000); //Math.round(distancia / 1000);
    showAlerta('<p>Kilometragem necessária: Km</p>');
}

function requestDirections(start, end) {
    var rendererOptions = {
        map: mapa,
        suppressMarkers: true,
        suppressInfoWindows: true
    };

    var directionsService = new google.maps.DirectionsService();

    requisicao = {
        origin: start,
        destination: end,
        optimizeWaypoints: false,
        travelMode: google.maps.DirectionsTravelMode.DRIVING,
        provideRouteAlternatives: false,
        avoidHighways: false,
        avoidTolls: false
    };

    directionsService.route(requisicao,
        function (response, status) {
            if (status === google.maps.GeocoderStatus.OVER_QUERY_LIMIT) {

                if (automatico === "S") {
                    window.opener = window;
                    window.close("#");
                }
                else {
                    alert("Google informa que sua cota de requisições foi ultrapassada.");
                }
            }

            if (status === google.maps.DirectionsStatus.OK) {

                var directionsDisplay = new google.maps.DirectionsRenderer(rendererOptions);
                directionsDisplay.setDirections(response);
                var rota = response.routes[0];
                var distancia = 0;

                for (var i = 0; i < rota.legs.length; i++) {
                    distancia = distancia + rota.legs[i].distance.value;
                }


                showAlerta('<p>Kilometragem necessária:  Km</p>');
            }
            else {

                if (automatico === "S") {
                    window.opener = window;
                    window.close("#");
                }
                else {
                    alert("Não foi possivel visualizar a rota.");
                }
            }
        }
    );
}

function TesteRota(pontosRota) {
    for (var x = 1; x <= pontosRota.length - 1; x++) {

        pontoInicio = getLatLng(pontosRota[x - 1].Latitude, pontosRota[x - 1].Longitude);
        setMarker('', getLatLng(pontosRota[x - 1].Latitude, pontosRota[x - 1].Longitude), false, builderConteudo(pontosRota[x - 1].NumOS, pontosRota[x - 1].Hora + '<br/>' + pontosRota[x - 1].Descricao), pontosRota[x - 1].Url);

        pontoFinal = getLatLng(pontosRota[x].Latitude, pontosRota[x].Longitude);
        setMarker('', getLatLng(pontosRota[x].Latitude, pontosRota[x].Longitude), false, builderConteudo(pontosRota[x].NumOS, pontosRota[x].Hora + '<br/>' + pontosRota[x].Descricao), pontosRota[x].Url);

        requestDirections(pontoInicio, pontoFinal);
    }

	/*var directionsService = new google.maps.DirectionsService();
   
	var rendererOptions = {
	  map: mapa,
	  suppressMarkers: true,
	  suppressInfoWindows: true
	};

	var pontoInicio;
	var pontoFinal;
	var distanciaRota = 0;
	var requisicao;

	for (var x = 1; x <= pontosRota.length-1; x++) 
	{
	   pontoInicio = getLatLng(pontosRota[x-1].Latitude,pontosRota[x-1].Longitude);
	   setMarker('',getLatLng(pontosRota[x-1].Latitude,pontosRota[x-1].Longitude),false,builderConteudo(pontosRota[x-1].NumOS,pontosRota[x-1].Hora+'<br/>'+pontosRota[x-1].Descricao),pontosRota[x-1].Url);

	   pontoFinal = getLatLng(pontosRota[x].Latitude,pontosRota[x].Longitude);
	   setMarker('',getLatLng(pontosRota[x].Latitude,pontosRota[x].Longitude),false,builderConteudo(pontosRota[x].NumOS,pontosRota[x].Hora+'<br/>'+pontosRota[x].Descricao),pontosRota[x].Url);

	   requisicao = {
			origin:pontoInicio,
			destination:pontoFinal,
			optimizeWaypoints: false,
			travelMode: google.maps.DirectionsTravelMode.DRIVING,
			provideRouteAlternatives: false,
			avoidHighways: false,
			avoidTolls: false
	   };

	   directionsService.route(requisicao,
			function(response, status) {
				if (status == google.maps.DirectionsStatus.OK) {

					var directionsDisplay = new google.maps.DirectionsRenderer(rendererOptions);
					directionsDisplay.setDirections(response);
					var rota = response.routes[0];
					var distancia = 0;

					for (var i = 0; i < rota.legs.length; i++) {
						distancia = distancia + rota.legs[i].distance.value;
					}

					distanciaRota = distanciaRota + (distancia / 1000); //Math.round(distancia / 1000);
					showAlerta('<p>Kilometragem necessária: ' + distanciaRota + ' Km</p>');
				}
				else {
					alert("Não foi possivel visualizar a rota.");
				}
			}
	   );
	}*/
}

function setLegendas(legendas) {
    for (var i = 0; i < legendas.length; i++) {
        setLegenda(legendas[i].Url, legendas[i].Descricao);
    }
}

function calcularRota(divMapa, jsonRota) {
    loadMap(divMapa, 0, 0);

    var itens = eval('(' + jsonRota + ')');

    //DesenharRota(itens.Marcadores,itens.MenorTrajeto, itens.TrajetosAlternativos, itens.EvitarRodovias, itens.EvitarPedagios);
    TesteRota(itens.Marcadores);

    setLegendas(itens.Legendas);
}

function showAlerta(mensagem) {
    $(function () {
        var alerta = $('#alerta');

        alerta.html(mensagem);

        alerta.show();
    }
    );
}

function setLegenda(urlIcone, legenda) {
    $(function () {
        lgd = $('#legenda');

        var html = lgd.html();

        html = html + '<p><img src="' + urlIcone + '"/> - ' + legenda + '</p>';

        lgd.html(html);

        lgd.show();
    }
    );
}

function CloseMapa() {
    $('#mapa').dialog('close');
}