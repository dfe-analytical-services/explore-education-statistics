/**
 * Define custom events here.
 */
interface GlobalEventHandlersEventMap {
  'ees:network_request': CustomEvent;
  'ees:network_request_error': CustomEvent;
  'ees:network_response': CustomEvent;
  'ees:network_response_error': CustomEvent;
}
