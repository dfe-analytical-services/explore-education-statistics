/* eslint-disable @typescript-eslint/no-explicit-any */
import HubResultError from '@admin/services/hubs/utils/HubResultError';
import isHubResult from '@admin/services/hubs/utils/isHubResult';
import logger from '@common/services/logger';
import { HubConnection, HubConnectionState } from '@microsoft/signalr';

export interface Subscription {
  methodName: string;
  callback(...args: any[]): void;
  unsubscribe(): void;
}

export default class Hub {
  constructor(protected connection: HubConnection) {}

  status(): HubConnectionState {
    return this.connection.state;
  }

  async start(): Promise<void> {
    if (this.connection.state !== 'Connected') {
      await this.connection.start();
    }
  }

  async stop(): Promise<void> {
    if (this.connection.state === 'Connected') {
      await this.connection.stop();
    }
  }

  /**
   * Calls a hub method and waits for a result.
   *
   * Compared to {@see send}, two websocket messages are sent -
   * one from client to the server (request) and one from the server
   * to the client (response) i.e. bidirectional communication occurs.
   */
  async invoke<T>(methodName: string, ...args: unknown[]): Promise<T> {
    logger.debugTime(`Invoking hub method '${methodName}'`, {
      url: this.connection.baseUrl,
      args,
    });

    const result = await this.connection.invoke(methodName, ...args);

    logger.debugTime(`Received invoke result for '${methodName}'`, {
      url: this.connection.baseUrl,
      result,
    });

    // Automatically try and unwrap hub results for convenience.
    // We want this to be similar in behaviour to our Axios clients.
    if (isHubResult<T>(result)) {
      if (result.status >= 200 && result.status <= 299 && result.data) {
        return result.data;
      }

      throw new HubResultError(result, methodName);
    }

    return result as T;
  }

  /**
   * Call a hub method without waiting for a result.
   *
   * A single websocket message is sent from the client to the server
   * i.e. unidirectional communication occurs.
   */
  async send(methodName: string, ...args: unknown[]): Promise<void> {
    if (this.connection.state !== 'Connected') {
      return;
    }

    logger.debugTime(`Sending to hub method '${methodName}'`, {
      url: this.connection.baseUrl,
      args,
    });

    await this.connection.send(methodName, ...args);
  }

  subscribe(
    methodName: string,
    callback: (...args: any[]) => void,
  ): Subscription {
    const handler = (...args: any[]) => {
      logger.debugTime(`Received '${methodName}' event`, {
        url: this.connection.baseUrl,
        args,
      });

      callback(...args);
    };

    this.connection.on(methodName, handler);

    return {
      methodName,
      callback,
      unsubscribe: () => this.connection.off(methodName, handler),
    };
  }

  onDisconnect(callback: (error?: Error) => void): void {
    this.connection.onclose(callback);
  }

  onReconnected(callback: (connectionId?: string) => void): void {
    this.connection.onreconnected(callback);
  }

  onReconnecting(callback: (error?: Error) => void): void {
    this.connection.onreconnecting(callback);
  }
}
