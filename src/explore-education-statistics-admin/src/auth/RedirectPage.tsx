import { useEffect, useState } from 'react';

import { broadcastResponseToMainFrame } from '@azure/msal-browser/redirect-bridge';
import logger from '@common/services/logger';
import Page from '@admin/components/Page';
import ErrorMessage from '@common/components/ErrorMessage';

function RedirectPage() {
  const [error, setError] = useState<string | undefined>(undefined);

  useEffect(() => {
    broadcastResponseToMainFrame().catch(reason => {
      logger.debug('Error broadcasting response to main frame:', reason);
      setError('There was an error signing you in');
    });
  }, []);

  return (
    <Page caption="Please wait..." title="Signing in...">
      {error && (
        <div>
          <ErrorMessage announceError>{error}</ErrorMessage>
        </div>
      )}
    </Page>
  );
}

export default RedirectPage;
