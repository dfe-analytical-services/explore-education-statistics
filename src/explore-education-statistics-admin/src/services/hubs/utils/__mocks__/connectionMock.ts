import { Writeable } from '@common/types';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';
import { mock } from 'jest-mock-extended';

const connectionMock = mock<Writeable<HubConnection>>();

Object.defineProperty(connectionMock, 'state', {
  get() {
    return HubConnectionState.Disconnected;
  },
  configurable: true,
});

connectionMock.send.mockResolvedValue();
connectionMock.stop.mockResolvedValue();
connectionMock.send.mockResolvedValue();

export default connectionMock;
