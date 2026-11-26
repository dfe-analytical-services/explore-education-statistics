import { useTenant } from '@common/contexts/TenantContext';
import { ReleaseTypeSectionProps } from '@common/modules/release/components/ReleaseTypeSection';
import React from 'react';

export default function AdHocStatisticsSection({
  showHeading = true,
}: ReleaseTypeSectionProps) {
  const { officialStatisticsStandardsUrl } = useTenant();

  return (
    <>
      {showHeading && <h3>Ad hoc official statistics</h3>}
      <p>
        Ad hoc official statistics are one off publications that have been
        produced as far as possible in line with the Code of Practice for
        Statistics.
      </p>
      <p>This can be broadly interpreted to mean that these statistics are:</p>
      <ul>
        <li>managed impartially and objectively in the public interest</li>

        <li>meet identified user needs</li>

        <li>produced according to sound methods</li>

        <li>well explained and readily accessible</li>
      </ul>
      {officialStatisticsStandardsUrl && (
        <p>
          Find out more about the standards we follow to produce these
          statistics through our{' '}
          <a
            href={officialStatisticsStandardsUrl}
            rel="noopener noreferrer nofollow"
            target="_blank"
          >
            Standards for official statistics published by DfE guidance (opens
            in new tab)
          </a>
          .
        </p>
      )}
    </>
  );
}
