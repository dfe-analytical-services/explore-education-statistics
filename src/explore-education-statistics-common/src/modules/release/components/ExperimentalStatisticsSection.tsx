import React from 'react';

export default function ExperimentalStatisticsSection({
  showHeading = true,
}: {
  showHeading?: boolean;
}) {
  return (
    <>
      {showHeading && <h3>Experimental statistics</h3>}
      <p>
        These statistics are experimental statistics undergoing evaluation. They
        have been developed under the guidance of the Head of Profession for
        Statistics and published to involve users and stakeholders at an early
        stage in assessing their suitability and quality.
      </p>
      <p>
        Experimental official statistics have been produced as far as possible
        in line with the Code of Practice for Statistics.
      </p>
      <p>This can be broadly interpreted to mean that these statistics are:</p>
      <ul>
        <li>managed impartially and objectively in the public interest</li>

        <li>meet identified user needs</li>

        <li>produced according to sound methods</li>

        <li>well explained and readily accessible</li>
      </ul>
      <p>
        Find out more about the standards we follow to produce these statistics
        through our{' '}
        <a href="https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education">
          Standards for official statistics published by DfE guidance
        </a>
        .
      </p>
    </>
  );
}
