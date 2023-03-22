# mailslot_vb
Mailslots `client` and `server` realization on VB.NET.

## Usage 

- Mailslots server example:

```
Using serv As New MailslotServer("test_channel")
  Console.WriteLine(serv.GetNextMessage())
End Using
```

- Mailslots client example:
```
Using clt As New MailslotClient("test_channel")
  clt.SendMessage("Hello, mailslots")
End Using
```

## Links 

- [Mailslots and iChat reversing](https://soltau.ru/index.php/themes/dev/item/554-interfejs-mailslot-na-primere-raboty-chata-intranet-chat)
