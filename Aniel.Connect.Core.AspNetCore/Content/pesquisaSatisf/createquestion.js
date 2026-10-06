
//Criando área de Seção para pergunta
$("#iNewSection").click(function(){
  $("body").append(
  '<br>'+  
  '<div class="main main-raised">' +
    '<div class="section section-basic">' +
      '<div class="container">' +
        '<div id="iDivQuestion">' +
        '</div>' +
      '</div>' +
    '</div>' + 
  '</div>' +
  '<br><br>'
  )
});

//Criando questão ao clicar no botão
$("#iBtnCreateQuestion").click(function(){

  $("#iDivQuestion").append(
    '<textarea class="form-control titleQuestion" type="text" placeholder="Digite a pergunta"></textarea>' +
    //Botão dropdown - Tipo de resposta
    '<div id="iBtnTipoResposta" class="dropdown">' +
    '<button class="btn btn-info dropdown-toggle" type="button" id="dropdownMenuButton" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">' +
    'Tipo de resposta' +
    '</button>' +
    '<div class="dropdown-menu" aria-labelledby="dropdownMenuButton">' +
      '<a class="dropdown-item" id="iLinkRespostaCurta" href="#"><i class="material-icons">short_text</i> Resposta curta</a>' +
      '<a class="dropdown-item" id="iLinkParagrafo" href="#"><i class="material-icons">subject</i> Paragrafo</a>' +
      '<a class="dropdown-item" id="iLinkMultiplaEscolha" href="#"><i class="material-icons">check_box</i> Multipla escolha</a>' +
      '<a class="dropdown-item" id="iLinkCaixaSelecao" href="#"><i class="material-icons">radio_button_checked</i> Caixa de seleção</a>' +
      '<a class="dropdown-item" id="iLinkEscala" href="#"><i class="material-icons">linear_scale</i> Escala de 0 a 10</a>' +
      '<a class="dropdown-item" id="iLinkNivelSatisfacao" href="#"><i class="material-icons">insert_emoticon</i> Nível de satisfação</a>' +
      '<a class="dropdown-item" id="iLinkUpload" href="#"><i class="material-icons">cloud_upload</i> Upload de arquivo</a>' +
    '</div>' +
  '</div>'    
  );
  

  //Resposta Curta - Input text                       
  $("#iLinkRespostaCurta").click(function(){
    $("#iDivQuestion").append(
      '<input type="text" class="form-control" placeholder="Resposta curta"/>'
      )
  });
    
  //Paragrafo - Textarea                        
  $("#iLinkParagrafo").click(function(){
    $("#iDivQuestion").append(
      '<textarea class="form-control" type="text" placeholder="Paragrafo"></textarea>'
      )
  });

  //Multipla escolha - Checkbox                      
  $("#iLinkMultiplaEscolha").click(function(){
    $("#iDivQuestion").append(
      '<div class="form-check">' + 
      '<label class="form-check-label">' + 
        '<input class="form-check-input" type="checkbox" disabled value="">' +
        'Eficiente' + 
        '<span class="form-check-sign">' +
          '<span class="check"></span>' +
        '</span>' +
      '</label>' +
    '</div>'
    )
  });

  //Caixa de seleção - Radio                        
  $("#iLinkCaixaSelecao").click(function(){
  $("#iDivQuestion").append(
    '<div class="form-check form-check-radio">' + 
      '<label class="form-check-label">' + 
        '<input class="form-check-input" type="radio" name="exampleRadios" id="exampleRadios1" value="option1">' +
        '3 minutos' +
        '<span class="circle">' +
          '<span class="check"></span>' +
        '</span>' +
      '</label>' +
    '</div>'
    )
  });  

  //Escalad de 0 a 10 - Colors Buttons                        
  $("#iLinkEscala").click(function(){
    $("#iDivQuestion").append(
      '<p class="text-center">' + 
      '<button class="btn button btn red1" id="btn-q1">1</button>' + 
      '<button class="btn button btn red2" id="btn-q2">2</button>' + 
      '<button class="btn button btn red3" id="btn-q3">3</button>' + 
      '<button class="btn button btn orange4" id="btn-q4">4</button>' + 
      '<button class="btn button btn orange5" id="btn-q5">5</button>' + 
      '<button class="btn button btn orange6" id="btn-q6">6</button>' + 
      '<button class="btn button btn green7" id="btn-q7">7</button>' + 
      '<button class="btn button btn green8" id="btn-q8">8</button>' + 
      '<button class="btn button btn green9" id="btn-q9">9</button>' + 
      '<button class="btn button btn green10" id="btn-q10">10</button></p>'
    )
  });  

  //Nível de Satisfação - Emojis                        
  $("#iLinkNivelSatisfacao").click(function(){
    $("#iDivQuestion").append(
      '<div class="">' + 
        '<i class="em em-angry smile" data-toggle="tooltip" data-placement="top" title="Péssimo"></i>' +
        '<i class="em em-slightly_frowning_face smile" data-toggle="tooltip" data-placement="top" title="Ruim" id="ruim"></i>' +
        '<i class="em em-neutral_face smile" data-toggle="tooltip" data-placement="top" title="Neutro" id="neutro"></i>' +
        '<i class="em em-slightly_smiling_face smile" data-toggle="tooltip" data-placement="top" title="Bom" id="bom"></i>' +
        '<i class="em em-smiley smile" data-toggle="tooltip" data-placement="top" title="Ótimo" id="otimo"></i>' +
      '</div>'
    )
  });  

  //Upload - File                       
  $("#iLinkUpload").click(function(){
    $("#iDivQuestion").append(
      '<form>' +
        '<div class="form-group btn btn-outline-info">' + 
          '<label for="exampleFormControlFile1">Escolha o arquivo</label>' +
          '<input type="file" class="form-control-file" id="exampleFormControlFile1">' +
        '</div>' +
      '</form>'
    )
  });
          
  
});//./ iBtnCreateQuestion

