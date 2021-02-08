# CPqD ASR Recognizer  [![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

*O Recognizer é uma API para criação de aplicações de voz que utilizam o servidor CPqD ASR para reconhecimento de fala.*

Para maiores informações, consulte [a documentação do projeto](https://speechweb.cpqd.com.br/asr/docs).

## Instalação

Baixe o conteúdo do repositório em seu computador usando o comando abaixo.

    $ git clone https://github.com/CPqD/asr-sdk-dotnet

Acesse a pasta do projeto *CPqDASR* e gere a dll com o seguinte comando, após esse processo adicione a dll ao seu projeto.

    $ dotnet build -c release


## Exemplo

Para fazer uma transcrição, devemos inicialmente criar as credenciais. O usuário e senha são fornecidos pelo CPqD, caso a sua instalação do servidor ASR não tenha uma camada de autenticação você pode ignorar essa etapa.

```c#
var credentials = new Credentials
          {
              UserName = "dummyUser",
              Password = "dummyPassword"
          };
```

Em sequencia temos que criar as nossas configuraçãoes de transcrição, caso deseje utilizar paramentros diferentes, recomendamos ler a [sessão de configuração](https://speechweb.cpqd.com.br/asr/docs/latest/config_asr/recog.html).
```c#
var recogConfig = new RecognitionConfig
          {
              ConfidenceThreshold = 70,
              Textify = true
          };
```

Com as nossas credenciais e configuração feitas, podemos instâncias o nosso cliente.
```c#
var clientConfig = new ClientConfig
          {
              ServerUrl = "wss://speech.cpqd.com.br/asr/ws/v2/recognize/8k",
              RecogConfig = recogConfig,
              UserAgent = "desktop=x64; os=Windows; client=.NET Core; app=Exemplo",
              Credentials = credentials,
              MaxWaitSeconds = 20000,
              ConnectOnRecognize = true,
          };
```

Devemos agora carregar o arquivo que será transcrito e também devemos escolher um dos modelos da língua.
```c#
var audioSource = new FileAudioSource("C:\\path\\to\\file\\audio.wav", AudioType.DETECT);
```

```c#
var languageModelList = new LanguageModelList();

languageModelList.AddFromUri("builtin:slm/general");
```

Por fim, podemos iniciar o processo de transcrição.
```c#
var recognizer = SpeechRecognizer.Create(clientConfig);

recognizer.Recognize(audioSource, languageModelList);
```

E para recuperar o resultado da transcrição.

```c#
results = recognizer.WaitRecognitionResult();
```

Caso queira imprimir no terminal o resultado.

```c#
Console.WriteLine(results[0].Alternatives[0].Text);
```

## Dependências
Para o funcionamento deste cliente é necessários a instalação da seguinte dependência.

* [`Newtonsoft.Json`](https://github.com/JamesNK/Newtonsoft.Json)


Licença
-------

Copyright (c) 2021 CPqD. Todos os direitos reservados.

Publicado sob a licença Apache Software 2.0, veja LICENSE.
