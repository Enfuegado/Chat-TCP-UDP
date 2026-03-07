<a name="readme-top"></a>

[![Unity][unity-shield]][unity-url]
[![CSharp][csharp-shield]][csharp-url]
[![dotNET][dotnet-shield]][dotnet-url]
[![Windows][windows-shield]][windows-url]
[![Sockets][sockets-shield]][sockets-url]
[![TextMeshPro][tmp-shield]][tmp-url]
[![MIT License][license-shield]][license-url]
[![Status][status-shield]][status-url]

<br />
<div align="center">
  <a href="https://github.com/tu-usuario/Chat-TCP-UDP">
    <img src="images/logo.png" alt="Logo" width="120" height="120">
  </a>

  <h3 align="center">Chat TCP/UDP</h3>

  <p align="center">
    Aplicacion de chat en tiempo real con soporte para TCP y UDP, envio de mensajes, imagenes y archivos — construida en Unity con C#.
    <br />
    <a href="https://github.com/tu-usuario/Chat-TCP-UDP"><strong>Explorar el codigo »</strong></a>
    <br />
    <br />
    <a href="#uso">Ver Demo</a>
    &middot;
    <a href="https://github.com/tu-usuario/Chat-TCP-UDP/issues/new?labels=bug">Reportar Bug</a>
    &middot;
    <a href="https://github.com/tu-usuario/Chat-TCP-UDP/issues/new?labels=enhancement">Solicitar Feature</a>
  </p>
</div>

---

## Tabla de Contenidos

1. [Sobre el Proyecto](#sobre-el-proyecto)
   - [Construido Con](#construido-con)
   - [Color Reference](#color-reference)
2. [Arquitectura](#arquitectura)
3. [TCP vs UDP](#tcp-vs-udp)
4. [Envio de Mensajes y Archivos](#envio-de-mensajes-y-archivos)
5. [Primeros Pasos](#primeros-pasos)
   - [Prerrequisitos](#prerrequisitos)
   - [Instalacion](#instalacion)
6. [Uso](#uso)
7. [Roadmap](#roadmap)
8. [Investigacion](#investigacion)
9. [Contacto](#contacto)
10. [Reconocimientos](#reconocimientos)

---

## Sobre el Proyecto

[![Screenshot del proyecto][product-screenshot]](https://github.com/tu-usuario/Chat-TCP-UDP)

**Chat TCP/UDP** es una implementacion de comunicacion en red por sockets integrada en Unity. Permite una sesion de chat en tiempo real entre un cliente y un servidor, con soporte para intercambiar mensajes de texto, imagenes y archivos como PDFs.

Caracteristicas principales:

- Cambio entre **TCP y UDP en tiempo de ejecucion** sin reiniciar la sesion, mediante un boton en la interfaz.
- **Protocolo binario personalizado** (`NetworkPacket` + `PacketSerializer`) que serializa cualquier tipo de dato sin el overhead de codificaciones como Base64.
- **Handshake manual sobre UDP** para simular el establecimiento de conexion que ese protocolo no provee de forma nativa.
- Despacho seguro de eventos de red al **hilo principal de Unity** mediante `MainThreadDispatcher`.

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

### Construido Con

[![Unity][unity-shield]][unity-url]
[![CSharp][csharp-shield]][csharp-url]
[![dotNET][dotnet-shield]][dotnet-url]
[![Windows][windows-shield]][windows-url]

| Tecnologia | Uso en el proyecto |
|---|---|
| **Unity 2022.3 LTS** | Motor principal, UI, ciclo de vida de objetos |
| **C# / .NET** | Logica de red, serializacion, controladores |
| **System.Net.Sockets** | `TcpClient`, `TcpListener`, `UdpClient` nativos |
| **TextMeshPro** | Interfaz de chat |
| **StandaloneFileBrowser** | Dialogos de apertura y guardado de archivos |

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

### Color Reference

Paleta visual usada en la interfaz del chat.

| Color | Hex | Uso |
|---|---|---|
| Background principal | `#1E1E2E` | Fondo de la ventana de chat |
| Burbuja propia | `#3B82F6` | Mensajes enviados por el usuario local |
| Burbuja remota | `#2D2D3F` | Mensajes recibidos del otro extremo |
| Texto principal | `#F8F8F2` | Texto de mensajes y etiquetas |
| Texto secundario | `#6272A4` | Timestamps y metadatos de archivo |
| Acento / botones | `#A78BFA` | Botones de accion (Send, Attach, Switch) |
| Error | `#FF5555` | Popup de error y estados de desconexion |
| Conectado | `#50FA7B` | Indicador de estado de conexion activa |

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## Arquitectura

El sistema sigue un modelo **Cliente <-> Servidor** donde ambos roles corren dentro de la misma escena de Unity como GameObjects separados, instanciados dinamicamente por `ChatBootstrapper` segun el protocolo activo.

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
| `ChatBootstrapper` | Instancia cliente y servidor segun protocolo; gestiona el cambio en caliente |
| `ClientBootstrapper` | Inicializa el cliente, conecta controlador y vista |
| `ServerBootstrapper` | Inicializa el servidor, enlaza controlador con vista |
| `ChatController` | Logica de negocio: envio y recepcion de mensajes, imagenes y archivos |
| `PacketSerializer` | Serializacion y deserializacion binaria de paquetes |
| `MainThreadDispatcher` | Despacha eventos de red al hilo principal de Unity via `ConcurrentQueue` |
| `TCPClient` / `TCPServer` | Implementacion TCP con `TcpClient` y `TcpListener` |
| `UDPClient` / `UDPServer` | Implementacion UDP con handshake propio sobre `UdpClient` |

### Interfaces

El diseno usa interfaces para desacoplar el transporte de la logica de negocio:

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

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## TCP vs UDP

### Comparativa general

| Caracteristica | TCP | UDP |
|---|---|---|
| Orientado a conexion | Si | No |
| Garantia de entrega | Si | No |
| Orden de paquetes | Garantizado | No garantizado |
| Control de flujo | Si | No |
| Velocidad relativa | Mas lento | Mas rapido |
| Limite de payload | Sin limite practico | ~60 KB (esta implementacion) |
| Uso ideal | Archivos, mensajes criticos | Mensajes cortos, baja latencia |

### TCP en este proyecto

Usa `TcpListener` para aceptar la conexion y un `NetworkStream` para la transferencia de datos. La deserializacion es completamente asincrona mediante `ReadExactAsync`, que garantiza leer exactamente los bytes esperados sin condiciones de carrera:

```csharp
// TCPServer.cs
tcpListener = new TcpListener(IPAddress.Any, port);
tcpListener.Start();
connectedClient = await tcpListener.AcceptTcpClientAsync();
networkStream   = connectedClient.GetStream();
```

### UDP en este proyecto

UDP no tiene estado de conexion, por lo que se implemento un **handshake manual** con `PacketType.Connect`. El cliente envia paquetes de conexion cada segundo hasta recibir un ACK del servidor:

```csharp
// UDPClient.cs
while (!IsConnected && udpClient != null)
{
    var connectPacket = new NetworkPacket(PacketType.Connect, Array.Empty<byte>());
    await SendRaw(connectPacket);
    await Task.Delay(1000);
}
```

El servidor responde con su propio paquete `Connect` como acuse de recibo y almacena el `IPEndPoint` remoto para los envios siguientes.

### Limite de tamano en UDP

UDP permite hasta 65 507 bytes de payload sobre IPv4, pero en redes con MTU estandar de 1 500 bytes los datagramas grandes se fragmentan a nivel IP. La perdida de cualquier fragmento invalida el datagrama completo. Por eso se establecio un limite de **60 KB por paquete**:

```csharp
private const int MaxPayloadSize = 60 * 1024;

if (data.Length > MaxPayloadSize)
    throw new InvalidOperationException("File exceeds protocol size limit (60KB).");
```

> Para enviar archivos pesados se recomienda usar TCP.

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## Envio de Mensajes y Archivos

Toda comunicacion se basa en `NetworkPacket`, que encapsula tipo, nombre de archivo y payload en bytes.

### Tipos de paquete

```csharp
public enum PacketType : byte
{
    Text    = 0,
    Image   = 1,
    File    = 2,
    Connect = 4   // Handshake interno UDP
}
```

### Formato binario del paquete

```
+----------+-------------+----------+--------------+----------+
|  1 byte  |   4 bytes   |  N bytes |   4 bytes    |  M bytes |
| PackType | nameLength  | fileName |  dataLength  |   data   |
+----------+-------------+----------+--------------+----------+
```

El esquema de **longitud prefijada** permite reconstruir cualquier paquete en el receptor sin ambiguedad, sin importar el tipo de archivo.

### Envio de mensaje de texto

```csharp
var packet = new NetworkPacket(
    PacketType.Text,
    Encoding.UTF8.GetBytes(message)
);
await connection.SendMessageAsync(packet);
```

### Envio de imagen (.png, .jpg)

```csharp
byte[] data = await File.ReadAllBytesAsync(path);
var packet   = new NetworkPacket(PacketType.Image, data, Path.GetFileName(path));
await connection.SendMessageAsync(packet);
```

### Envio de archivo (.pdf y otros formatos)

```csharp
byte[] data = await File.ReadAllBytesAsync(path);
var packet   = new NetworkPacket(PacketType.File, data, Path.GetFileName(path));
await connection.SendMessageAsync(packet);
```

El receptor identifica el tipo de paquete y llama al metodo de vista correspondiente (`DisplayText`, `DisplayImage`, `DisplayFile`). Para archivos recibidos, se muestra un componente con nombre, tamano en KB y boton de descarga via `StandaloneFileBrowser`.

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## Primeros Pasos

### Prerrequisitos

- [Unity 2022.3 LTS](https://unity.com/releases/lts) o superior
- Windows 10 / 11
- Git

El paquete `StandaloneFileBrowser` debe estar presente en el proyecto. Si no esta, agregarlo desde:

```
https://github.com/gkngkc/UnityStandaloneFileBrowser
```

### Instalacion

1. Clonar el repositorio

   ```bash
   git clone https://github.com/tu-usuario/Chat-TCP-UDP.git
   ```

2. Abrir **Unity Hub**, seleccionar **Open Project** y navegar a la carpeta clonada.

3. Esperar a que Unity importe todos los assets y paquetes.

4. Abrir la escena principal desde:

   ```
   Assets/Scenes/MainScene.unity
   ```

5. Seleccionar el GameObject `ChatBootstrapper` en la jerarquia y verificar que los cuatro prefabs esten asignados en el Inspector:

   | Campo | Prefab esperado |
   |---|---|
   | `tcpClientPrefab` | TCP Client |
   | `tcpServerPrefab` | TCP Server |
   | `udpClientPrefab` | UDP Client |
   | `udpServerPrefab` | UDP Server |

6. Presionar **Play**. El servidor y el cliente se inicializan automaticamente.

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## Uso

Una vez en Play Mode, la interfaz permite:

- **Enviar mensajes de texto** con el campo de entrada y el boton *Send*.
- **Enviar una imagen** (.png, .jpg) con el boton *Send Image*, que abre un dialogo nativo de seleccion.
- **Adjuntar un archivo** (.pdf u otros) con el boton *Attach File*.
- **Cambiar de protocolo** entre TCP y UDP con el boton *Switch Protocol*. Esto desconecta la sesion actual y recrea cliente y servidor con el protocolo seleccionado.

[![Demo del chat][demo-screenshot]](https://youtu.be/LINK_AL_VIDEO)

[Ver video de demostracion completo](https://youtu.be/LINK_AL_VIDEO)

Para probar cliente y servidor como procesos separados, generar una build desde `File > Build Settings > Build` y ejecutar simultaneamente el editor y el ejecutable generado.

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## Roadmap

- [x] Comunicacion TCP cliente-servidor
- [x] Comunicacion UDP con handshake manual
- [x] Cambio de protocolo en tiempo de ejecucion
- [x] Envio de mensajes de texto
- [x] Envio de imagenes
- [x] Envio de archivos (PDF y otros formatos binarios)
- [ ] Soporte para multiples clientes simultaneos
- [ ] Indicador de progreso en transferencia de archivos
- [ ] Fragmentacion de payloads grandes en UDP
- [ ] Cifrado de mensajes

Ver los [issues abiertos](https://github.com/tu-usuario/Chat-TCP-UDP/issues) para la lista completa de mejoras propuestas.

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## Investigacion

### Soporte de multiples formatos de archivo

El primer problema fue determinar como representar de forma uniforme archivos tan distintos como texto plano, imagenes y PDFs. Despues de revisar la documentacion de `System.IO` en .NET y varios ejemplos de transferencia de archivos por sockets, quedo claro que el enfoque mas directo es **leer todo archivo como un arreglo de bytes** y dejar la interpretacion al receptor:

```csharp
byte[] data = await File.ReadAllBytesAsync(path);
```

Un PDF, una imagen PNG y un ZIP son, a nivel de red, indistinguibles: todos son secuencias de bytes. El nombre del archivo viaja junto al paquete para que el receptor pueda guardarlo con la extension correcta.

### Serializacion binaria vs JSON

Al principio se evaluo usar JSON para serializar los paquetes por su simplicidad de depuracion. El problema es que JSON no puede representar bytes arbitrarios directamente: requiere codificacion Base64, que incrementa el tamano del payload cerca de un 33%. Dado que UDP tiene un limite estricto de tamano de datagrama, esa sobrecarga resultaba inaceptable.

La solucion fue un **serializador binario propio** con esquema de longitud prefijada, usando `BinaryWriter` y `BinaryReader` sobre un `MemoryStream`. Este patron permite deserializar cualquier paquete de forma determinista sin necesidad de delimitadores ni parsing:

```csharp
writer.Write((byte)packet.Type);    // 1 byte  — tipo de paquete
writer.Write(nameBytes.Length);     // 4 bytes — longitud del nombre
writer.Write(nameBytes);            // N bytes — nombre del archivo
writer.Write(packet.Data.Length);   // 4 bytes — longitud del payload
writer.Write(packet.Data);          // M bytes — contenido del archivo
```

### Limite de 60 KB en UDP y fragmentacion IP

La especificacion de UDP (RFC 768) permite hasta 65 507 bytes de payload sobre IPv4, pero en redes con MTU estandar de 1 500 bytes los datagramas que superan ese valor se fragmentan a nivel IP. Cada fragmento viaja de forma independiente y la perdida de uno invalida el datagrama completo, lo que aumenta la tasa de fallos en transferencias grandes.

Consultando la documentacion de `System.Net.Sockets` de Microsoft y haciendo pruebas en red local con archivos de distintos tamanos, se definio **60 KB como limite conservador** que evita la fragmentacion en condiciones normales de red.

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## Estructura del Proyecto

```
Chat-TCP-UDP/
+-- Assets/
|   +-- Scripts/
|       +-- Bootstrap/
|       |   +-- ChatBootstrapper.cs
|       |   +-- ClientBootstrapper.cs
|       |   +-- ServerBootstrapper.cs
|       +-- Controllers/
|       |   +-- ChatController.cs
|       +-- Core/
|       |   +-- NetworkPacket.cs
|       |   +-- PacketSerializer.cs
|       |   +-- PacketType.cs
|       |   +-- ProtocolType.cs
|       +-- Interfaces/
|       |   +-- IChatConnection.cs
|       |   +-- IChatView.cs
|       |   +-- IClient.cs
|       |   +-- IServer.cs
|       +-- Network/
|       |   +-- TCPClient.cs
|       |   +-- TCPServer.cs
|       |   +-- UDPClient.cs
|       |   +-- UDPServer.cs
|       +-- UI/
|       |   +-- ChatInputHandler.cs
|       |   +-- ChatUIActions.cs
|       |   +-- ChatUIView.cs
|       |   +-- ClientConnectionUIHandler.cs
|       |   +-- ErrorPopupUI.cs
|       |   +-- FileMessageUI.cs
|       +-- Utils/
|           +-- MainThreadDispatcher.cs
+-- images/
+-- screenshots/
+-- README.md
```

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## Contacto

Tu Nombre - [@tu_twitter](https://twitter.com/tu_usuario) - tu@email.com

Link del proyecto: [https://github.com/tu-usuario/Chat-TCP-UDP](https://github.com/tu-usuario/Chat-TCP-UDP)

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## Reconocimientos

- [Microsoft Docs — System.Net.Sockets](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets)
- [RFC 768 — User Datagram Protocol](https://datatracker.ietf.org/doc/html/rfc768)
- [RFC 793 — Transmission Control Protocol](https://datatracker.ietf.org/doc/html/rfc793)
- [StandaloneFileBrowser para Unity](https://github.com/gkngkc/UnityStandaloneFileBrowser)
- [Best README Template](https://github.com/othneildrew/Best-README-Template)
- [Shields.io](https://shields.io)

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

## Licencia

Distribuido bajo la licencia MIT. Ver `LICENSE.txt` para mas informacion.

<p align="right">(<a href="#readme-top">volver arriba</a>)</p>

---

<!-- MARKDOWN LINKS & IMAGES -->
[unity-shield]: https://img.shields.io/badge/Unity-2022.3_LTS-black?style=for-the-badge&logo=unity&logoColor=white
[unity-url]: https://unity.com

[csharp-shield]: https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white
[csharp-url]: https://learn.microsoft.com/en-us/dotnet/csharp/

[dotnet-shield]: https://img.shields.io/badge/.NET-6.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white
[dotnet-url]: https://dotnet.microsoft.com

[windows-shield]: https://img.shields.io/badge/Windows-10%2F11-0078D4?style=for-the-badge&logo=windows&logoColor=white
[windows-url]: https://www.microsoft.com/windows

[sockets-shield]: https://img.shields.io/badge/Sockets-TCP%20%7C%20UDP-orange?style=for-the-badge&logo=cloudflare&logoColor=white
[sockets-url]: https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets

[tmp-shield]: https://img.shields.io/badge/TextMeshPro-UI-blueviolet?style=for-the-badge&logo=unity&logoColor=white
[tmp-url]: https://docs.unity3d.com/Manual/com.unity.textmeshpro.html

[license-shield]: https://img.shields.io/badge/License-MIT-green?style=for-the-badge
[license-url]: https://github.com/tu-usuario/Chat-TCP-UDP/blob/main/LICENSE.txt

[status-shield]: https://img.shields.io/badge/Status-Completed-brightgreen?style=for-the-badge
[status-url]: https://github.com/tu-usuario/Chat-TCP-UDP

[product-screenshot]: screenshots/chat-main.png
[demo-screenshot]: screenshots/demo-preview.png