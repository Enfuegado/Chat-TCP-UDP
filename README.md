<a name="readme-top"></a>

[![Unity](https://img.shields.io/badge/Unity-6000.3.8f1-black?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com/) [![CSharp](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/) [![dotNET](https://img.shields.io/badge/.NET-6.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/) [![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://www.microsoft.com/windows) [![Sockets](https://img.shields.io/badge/Sockets-TCP%20%7C%20UDP-orange?style=for-the-badge&logo=cloudflare&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets) [![TextMeshPro](https://img.shields.io/badge/TextMeshPro-UI-blueviolet?style=for-the-badge&logo=unity&logoColor=white)](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) [![MIT License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](https://github.com/tu-usuario/Chat-TCP-UDP/blob/main/LICENSE.txt) [![Status](https://img.shields.io/badge/Status-Completed-brightgreen?style=for-the-badge)](https://github.com/tu-usuario/Chat-TCP-UDP)

<br />
<div align="center">
  <a href="https://github.com/Enfuegado?tab=repositories">
    <img src="Images/Project Repository Overview.PNG" alt="Project Repository Overview" width="700">
  </a>

  <h3 align="center">Chat TCP/UDP</h3>

  <p align="center">
    Aplicación de chat en tiempo real con soporte para TCP y UDP, envío de mensajes, imágenes y archivos, construida en Unity con C#.
    <br />
    <a href="https://github.com/Enfuegado/Chat-TCP-UDP-base"><strong>Explorar el código »</strong></a>
  </p>
</div>

----------

## Tabla de Contenidos

1. [Sobre el Proyecto](#sobre-el-proyecto)
   - [Construido Con](#construido-con)
2. [Arquitectura](#arquitectura)
3. [TCP vs UDP](#tcp-vs-udp)
4. [Envío de Mensajes y Archivos](#envio-de-mensajes-y-archivos)
5. [Primeros Pasos](#primeros-pasos)
   - [Prerrequisitos](#prerrequisitos)
   - [Instalación](#instalacion)
6. [Uso](#uso)
7. [Roadmap](#roadmap)
8. [Investigación](#investigacion)
9. [Contacto](#contactos)
10. [Reconocimientos](#reconocimientos)

----------

## Sobre el Proyecto

<br />
<div align="center">
  <a href="https://github.com/Enfuegado?tab=repositories">
    <img src="Images/Main Menu Interface.PNG" alt="Main Menu Interface" width="700">
  </a>
</div>

**Chat TCP/UDP** es una implementación de comunicación en red por sockets integrada en Unity. Permite una sesión de chat en tiempo real entre un cliente y un servidor, con soporte para intercambiar mensajes de texto, imágenes y archivos como PDFs.

Características principales:

- Cambio entre **TCP y UDP en tiempo de ejecución**, reinicializando la conexión mediante un botón en la interfaz.
- **Protocolo binario personalizado** (`NetworkPacket` + `PacketSerializer`) que serializa cualquier tipo de dato sin el overhead de codificaciones como Base64.
- **Handshake manual sobre UDP** para simular el establecimiento de conexión que ese protocolo no provee de forma nativa.
- Despacho seguro de eventos de red al **hilo principal de Unity** mediante `MainThreadDispatcher`.

### Construido Con

[![Unity](https://img.shields.io/badge/Unity-6000.3.8f1-black?style=for-the-badge&logo=unity&logoColor=white)](https://unity.com/) [![CSharp](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/) [![dotNET](https://img.shields.io/badge/.NET-6.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/) [![Windows](https://img.shields.io/badge/Windows-10%2F11-0078D4?style=for-the-badge&logo=windows&logoColor=white)](https://www.microsoft.com/windows)

| Tecnología | Uso en el proyecto |
|---|---|
| **Unity 6000.3.8f1** | Motor principal, UI, ciclo de vida de objetos |
| **C# / .NET** | Lógica de red, serialización, controladores |
| **System.Net.Sockets** | `TcpClient`, `TcpListener`, `UdpClient` nativos |
| **TextMeshPro** | Interfaz de chat |
| **StandaloneFileBrowser** | Diálogos de apertura y guardado de archivos |

## Arquitectura

El sistema sigue un modelo **Cliente <-> Servidor** con una única conexión cliente-servidor; el servidor acepta solo un cliente conectado a la vez. El proyecto utiliza un flujo de escenas donde `MenuScene` carga un menú de selección de protocolo y, posteriormente, `ChatScene`. En esta última, cliente y servidor se ejecutan como GameObjects separados instanciados dinámicamente por `ChatBootstrapper` según el protocolo activo.

El protocolo elegido en el menú se almacena en `ProtocolSelection`, una clase compartida que mantiene el estado del protocolo activo entre escenas. Al iniciar `ChatScene`, `ChatBootstrapper` consulta este valor para instanciar dinámicamente los prefabs de red correspondientes.

El siguiente diagrama representa la arquitectura interna de `ChatScene`:

```
+----------------------------------------------------------+
|                       Unity Scene                        |
|                                                          |
|   +------------------+        +----------------------+   |
|   | ClientBootstrap  |        |  ServerBootstrapper  |   |
|   | ---------------- |        |  ------------------  |   |
|   |  TCPClient  -----+------->|  TCPServer           |   |
|   |  UDPClient  -----+------->|  UDPServer           |   |
|   +--------+---------+        +----------+-----------+   |
|            |                             |               |
|            v                             v               |
|   +----------------+         +-----------------+         |
|   | ChatController |         | ChatController  |         |
|   +--------+-------+         +--------+--------+         |
|            |                          |                  |
|            v                          v                  |
|   +----------------+         +-----------------+         |
|   |  ChatUIView    |         |   ChatUIView    |         |
|   +----------------+         +-----------------+         |
|                                                          |
|              Coordinado por: ChatBootstrapper            |
+----------------------------------------------------------+
```

### Componentes principales

| Clase | Responsabilidad |
|---|---|
| `ChatBootstrapper` | Instancia cliente y servidor según protocolo; gestiona el cambio en caliente |
| `ClientBootstrapper` | Inicializa el cliente, conecta controlador y vista |
| `ServerBootstrapper` | Inicializa el servidor, enlaza controlador con vista |
| `ChatController` | Lógica de negocio: envío y recepción de mensajes, imágenes y archivos |
| `PacketSerializer` | Serialización y deserialización binaria de paquetes |
| `MainThreadDispatcher` | Despacha eventos de red al hilo principal de Unity vía `ConcurrentQueue` |
| `TCPClient` / `TCPServer` | Implementación TCP con `TcpClient` y `TcpListener` |
| `UDPClient` / `UDPServer` | Implementación UDP con handshake propio sobre `UdpClient` |

### Interfaces

El diseño usa interfaces para desacoplar el transporte de la lógica de negocio:

```csharp
public interface IChatConnection
{
    event Action<NetworkPacket> OnPacketReceived;
    event Action OnConnected;
    event Action OnDisconnected;
    event Action<string> OnError;
    bool IsConnected { get; }
    Task SendMessageAsync(NetworkPacket packet);
    void Disconnect();
}

public interface IServer : IChatConnection { Task StartServer(int port); }
public interface IClient : IChatConnection { Task ConnectToServer(string ip, int port); }
```

----------

## TCP vs UDP

### Comparativa general

| Característica | TCP | UDP |
|---|---|---|
| Orientado a conexión | Sí | No |
| Garantía de entrega | Sí | No |
| Orden de paquetes | Garantizado | No garantizado |
| Control de flujo | Sí | No |
| Velocidad relativa | Más lento | Más rápido |
| Límite de payload | Sin límite práctico | ~60 KB (esta implementación) |
| Uso ideal | Archivos, mensajes críticos | Mensajes cortos, baja latencia |

### TCP en este proyecto

Usa `TcpListener` para aceptar la conexión y un `NetworkStream` para la transferencia de datos. La deserialización es completamente asíncrona mediante `ReadExactAsync`, que garantiza leer exactamente los bytes esperados sin condiciones de carrera:

```csharp
// TCPServer.cs
tcpListener = new TcpListener(IPAddress.Any, port);
tcpListener.Start();
connectedClient = await tcpListener.AcceptTcpClientAsync();
networkStream   = connectedClient.GetStream();
```

### UDP en este proyecto

UDP no tiene estado de conexión, por lo que se implementó un **handshake manual** con `PacketType.Connect`. El cliente envía paquetes de conexión cada segundo hasta recibir un ACK del servidor:

```csharp
// UDPClient.cs
while (!IsConnected && udpClient != null)
{
    var connectPacket = new NetworkPacket(PacketType.Connect, Array.Empty<byte>());
    await SendRaw(connectPacket);
    await Task.Delay(1000);
}
```

El servidor responde con su propio paquete `Connect` como acuse de recibo y almacena el `IPEndPoint` remoto para los envíos siguientes.

### Límite de tamaño en UDP
UDP permite hasta 65 507 bytes de payload sobre IPv4. Sin embargo, en redes con MTU estándar de aproximadamente 1 500 bytes, los datagramas grandes se fragmentan a nivel IP. La pérdida de cualquier fragmento invalida el datagrama completo. Por esta razón se estableció un límite de **60 KB por paquete**, manteniéndose dentro del tamaño máximo permitido por UDP aunque estos datagramas puedan fragmentarse en redes con MTU estándar:

```csharp
private const int MaxPayloadSize = 60 * 1024;

if (data.Length > MaxPayloadSize)
    throw new InvalidOperationException("File exceeds protocol size limit (60KB).");
```

> Para enviar archivos pesados se recomienda usar TCP.

----------

## Envío de Mensajes y Archivos

Toda comunicación se basa en `NetworkPacket`, que encapsula tipo, nombre de archivo y payload en bytes.

### Tipos de paquete

```csharp
public enum PacketType : byte
{
    Text    = 0,
    Image   = 1,
    File    = 2,
    Connect = 4
}
```

### Formato binario del paquete

```
+----------+-------------+----------+--------------+----------+
|  1 byte  |   4 bytes   |  N bytes |   4 bytes    |  M bytes |
| PackType | nameLength  | fileName |  dataLength  |   data   |
+----------+-------------+----------+--------------+----------+
```

El esquema de **longitud prefijada** permite reconstruir cualquier paquete en el receptor sin ambigüedad, sin importar el tipo de archivo.

### Envío de mensaje de texto

```csharp
var packet = new NetworkPacket(
    PacketType.Text,
    Encoding.UTF8.GetBytes(message)
);
await connection.SendMessageAsync(packet);
```

### Envío de imagen (.png, .jpg)

```csharp
byte[] data = await File.ReadAllBytesAsync(path);
var packet   = new NetworkPacket(PacketType.Image, data, Path.GetFileName(path));
await connection.SendMessageAsync(packet);
```

### Envío de archivo (.pdf y otros formatos)

```csharp
byte[] data = await File.ReadAllBytesAsync(path);
var packet   = new NetworkPacket(PacketType.File, data, Path.GetFileName(path));
await connection.SendMessageAsync(packet);
```

El paquete recibido es emitido por la capa de red mediante el evento `OnPacketReceived`, que es manejado por `ChatController`. El controlador interpreta el tipo de paquete y delega la actualización de la interfaz a la vista (`DisplayText`, `DisplayImage`, `DisplayFile`). Para archivos recibidos, se muestra un componente con nombre, tamaño en KB y un botón de descarga utilizando `StandaloneFileBrowser`.

----------

## Primeros Pasos

### Prerrequisitos

- [Unity 6000.3.8f1](https://unity.com/releases/editor/whats-new/6000.3.8) o superior
- Windows 10 / 11
- Git

El paquete `StandaloneFileBrowser` debe estar presente en el proyecto. Si no está, agregarlo desde:

```
https://github.com/gkngkc/UnityStandaloneFileBrowser
```

### Instalación

1. Clonar el repositorio

    ```bash
    git clone https://github.com/tu-usuario/Chat-TCP-UDP.git
    ```

2. Abrir **Unity Hub**, seleccionar **Open Project** y navegar a la carpeta clonada.

3. Esperar a que Unity importe todos los assets y paquetes.

4. Abrir la escena inicial del proyecto desde:

    ```
    Assets/Scenes/MenuScene.unity
    ```

5. Elegir **TCP** o **UDP** en el menú. Esto cargará automáticamente la escena de chat:

    ```
    Assets/Scenes/Chat/ChatScene.unity
    ```

6. Al iniciar `ChatScene`, el `ChatBootstrapper` instancia dinámicamente los prefabs de red correspondientes al protocolo seleccionado. Verificar en el Inspector que los cuatro prefabs estén asignados correctamente:

    | Campo | Prefab esperado |
    |---|---|
    | `tcpClientPrefab` | TCP Client |
    | `tcpServerPrefab` | TCP Server |
    | `udpClientPrefab` | UDP Client |
    | `udpServerPrefab` | UDP Server |

7. Cliente y servidor se inicializan automáticamente dentro de la escena para pruebas locales.

----------

## Uso

Una vez en Play Mode, la interfaz permite:

- **Enviar mensajes de texto** con el campo de entrada y el botón _Send_.
- **Enviar una imagen** (.png, .jpg) con el botón _Send Image_, que abre un diálogo nativo de selección.
- **Adjuntar un archivo** (.pdf u otros) con el botón _Attach File_.
- **Cambiar de protocolo** entre TCP y UDP con el botón _Switch Protocol_. Esto desconecta la sesión actual y recrea cliente y servidor con el protocolo seleccionado.

Para probar cliente y servidor como procesos separados, generar una build desde `File > Build Settings > Build` y ejecutar simultáneamente el editor y el ejecutable generado.

### Interfaz TCP

<div align="center">
  <img src="Images/TCP Chat Interface.PNG" alt="TCP Chat Interface" width="700">
</div>

### Interfaz UDP

<div align="center">
  <img src="Images/UDP Chat Interface.PNG" alt="UDP Chat Interface" width="700">
</div>

----------

## Investigación

### Soporte de múltiples formatos de archivo

El primer problema fue determinar cómo representar de forma uniforme archivos tan distintos como texto plano, imágenes y PDFs. Después de revisar la documentación de `System.IO` en .NET y varios ejemplos de transferencia de archivos por sockets, quedó claro que el enfoque más directo es **leer todo archivo como un arreglo de bytes** y dejar la interpretación al receptor:

```csharp
byte[] data = await File.ReadAllBytesAsync(path);
```

Un PDF, una imagen PNG y un ZIP son, a nivel de red, indistinguibles: todos son secuencias de bytes. El nombre del archivo viaja junto al paquete para que el receptor pueda guardarlo con la extensión correcta.

### Límite de 60 KB en UDP y fragmentación IP

La especificación de UDP (RFC 768) permite hasta 65 507 bytes de payload sobre IPv4, pero en redes con MTU estándar de 1 500 bytes los datagramas que superan ese valor se fragmentan a nivel IP. Cada fragmento viaja de forma independiente y la pérdida de uno invalida el datagrama completo, lo que aumenta la tasa de fallos en transferencias grandes.

Consultando la documentación de `System.Net.Sockets` de Microsoft y haciendo pruebas en red local con archivos de distintos tamaños, se definió **60 KB como límite conservador** que evita la fragmentación en condiciones normales de red.

----------

## Estructura del Proyecto

```
Chat-TCP-UDP/
├── Assets/
│   ├── Scripts/
│   │   ├── Chat/
│   │   │   ├── Bootstrap/
│   │   │   │   ├── ChatBootstrapper.cs
│   │   │   │   ├── ClientBootstrapper.cs
│   │   │   │   └── ServerBootstrapper.cs
│   │   │   ├── Controller/
│   │   │   │   └── ChatController.cs
│   │   │   └── View/
│   │   │       ├── Actions/
│   │   │       │   └── ChatUIActions.cs
│   │   │       ├── Handlers/
│   │   │       │   ├── ChatInputHandler.cs
│   │   │       │   └── ClientConnectionUIHandler.cs
│   │   │       ├── Views/
│   │   │       │   └── ChatUIView.cs
│   │   │       └── Widgets/
│   │   │           ├── ErrorPopupUI.cs
│   │   │           └── FileMessageUI.cs
│   │   ├── Contracts/
│   │   │   ├── ConnectionStatus.cs
│   │   │   ├── IChatConnection.cs
│   │   │   ├── IChatView.cs
│   │   │   ├── IClient.cs
│   │   │   └── IServer.cs
│   │   ├── Core/
│   │   │   ├── ProtocolSelection.cs
│   │   │   └── MainThreadDispatcher.cs
│   │   ├── Menu/
│   │   │   └── ProtocolMenuController.cs
│   │   └── Networking/
│   │       ├── Protocol/
│   │       │   ├── NetworkPacket.cs
│   │       │   ├── PacketSerializer.cs
│   │       │   ├── PacketType.cs
│   │       │   └── ProtocolType.cs
│   │       ├── TCP/
│   │       │   ├── TCPClient.cs
│   │       │   └── TCPServer.cs
│   │       └── UDP/
│   │           ├── UDPClient.cs
│   │           └── UDPServer.cs
│   ├── Plugins/
│   │   └── StandaloneFileBrowser/
│   ├── Prefabs/
│   │   ├── Networking/
│   │   └── UI/
│   ├── Scenes/
│   │   ├── Chat/
│   │   │   └── ChatScene
│   │   └── Menu/
│   │       └── MenuScene
│   └── UxUiAssets/
│       ├── Fonts/
│       ├── GeneralAssets/
│       ├── TCPAssets/
│       └── UDPAssets/
├── images/
└── README.md
```

----------

## Contactos

Jaime Barragán — [@Enfuegado](https://github.com/Enfuegado)  
Paula Castro — [@PonkinaDev](https://github.com/PonkinaDev)

----------

## Reconocimientos

- [Microsoft Docs — System.Net.Sockets](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets)
- [RFC 768 — User Datagram Protocol](https://datatracker.ietf.org/doc/html/rfc768)
- [RFC 793 — Transmission Control Protocol](https://datatracker.ietf.org/doc/html/rfc793)
- [Best README Template](https://github.com/othneildrew/Best-README-Template)
