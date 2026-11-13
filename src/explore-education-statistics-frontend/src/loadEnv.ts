import configureApiClients from '@frontend/services/clients/configureApiClients';

// Must run before any API requests are made, so this
// module should be imported first in `_app`.
configureApiClients();
