# Exchanges

A exchange é basicamente um sistema de roatementos.
Quando a exchang é utilizada, o produtor bate na exchange, e não na fila.
Quando não é especificado algum tipo de exchange, é utilizada a exchange default do RabbitMQ.

## Fanout

Utilizado quando preciso fazer cópias da mesma mensagem para mais de uma fila. (réplica)

O tipo de exchange 'fanout' ignora o parâmetro 'routingKey'.

![img_1.png](img_1.png)