import ButtonText from '@common/components/ButtonText';
import Modal from '@common/components/Modal';
import React from 'react';

export default function APIDatasetsModal() {
  return (
    <Modal
      showClose
      title="API data sets guidance"
      triggerButton={<ButtonText>What are API data sets?</ButtonText>}
    >
      <section>
        <h3>API data sets</h3>
        <p>
          Select this option to filter for data sets that are specifically
          compatible with the Explore Education Statistics API. These data sets
          are structured with machine-readable identifiers and versioning to
          support automated data feeds.
        </p>

        <ul>
          <li>
            Best for: Developers and data scientists building live dashboards,
            apps, or automated reporting pipelines that need to "pull" data
            directly into their own systems.
          </li>

          <li>
            What’s included: A curated subset of data sets that have been
            optimised for API access. Note that while many data sets are
            available via the API, some older or highly specialised files may
            only be available as manual downloads under "All data sets".
          </li>
        </ul>
      </section>

      <section>
        <h3>All Data Sets</h3>

        <p>
          Select this option to browse the complete library of education
          statistics. This includes every data file available for download on
          the service, ranging from large-scale national trends to specific
          local authority breakdowns.
        </p>

        <ul>
          <li>
            Best for: General users, researchers, and policy analysts who want
            to download CSV or ZIP files to use in Excel, SQL, or other desktop
            analysis tools.
          </li>

          <li>
            What’s included: The full historical archive and the latest releases
            across all themes (e.g., school performance, attendance, and social
            care).
          </li>
        </ul>
      </section>
    </Modal>
  );
}
