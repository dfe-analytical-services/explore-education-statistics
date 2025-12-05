import { ReleaseTypeSectionProps } from '@common/modules/release/components/ReleaseTypeSection';
import React from 'react';

export default function OfficialStatisticsInDevelopmentSection({
  publishingOrganisations,
  showHeading = true,
}: ReleaseTypeSectionProps) {
  const isSkillsEngland = !!publishingOrganisations?.find(
    org => org.title === 'Skills England',
  );
  return (
    <>
      {showHeading && <h3>Official statistics in development</h3>}
      {isSkillsEngland ? (
        <p>
          These statistics are undergoing a development. They have been
          developed by Skills England under the guidance of Skills England Lead
          Official for Statistics, and published to involve users and
          stakeholders at an early stage in assessing their suitability and
          quality.
        </p>
      ) : (
        <p>
          These statistics are undergoing a development. They have been
          developed under the guidance of the Head of Profession for Statistics
          and published to involve users and stakeholders at an early stage in
          assessing their suitability and quality.
        </p>
      )}

      <p>
        They have been produced as far as possible in line with the Code of
        Practice for Statistics.
      </p>

      <p>This can be broadly interpreted to mean that these statistics are:</p>
      <ul>
        <li>managed impartially and objectively in the public interest</li>

        <li>meet identified user needs</li>

        <li>produced according to sound methods</li>

        <li>well explained and readily accessible</li>
      </ul>

      {isSkillsEngland ? (
        <p>
          Find out more about the standards we follow to produce these
          statistics through our{' '}
          <a
            href="https://www.gov.uk/government/publications/skills-england-standards-for-official-statistics"
            rel="noopener noreferrer nofollow"
            target="_blank"
          >
            Standards for official statistics published by Skills England
            guidance (opens in new tab)
          </a>
        </p>
      ) : (
        <p>
          Find out more about the standards we follow to produce these
          statistics through our{' '}
          <a
            href="https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education"
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
