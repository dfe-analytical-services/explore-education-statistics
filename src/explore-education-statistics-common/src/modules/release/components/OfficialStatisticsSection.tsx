import React from 'react';

export default function OfficialStatisticsSection({
  showHeading = true,
}: {
  showHeading?: boolean;
}) {
  return (
    <>
      {showHeading && <h3>Official statistics</h3>}
      <p>
        These are Official Statistics and have been produced in line with the{' '}
        <a
          href="https://code.statisticsauthority.gov.uk/the-code/"
          rel="noopener noreferrer nofollow"
          target="_blank"
        >
          Code of Practice for Official Statistics (opens in new tab)
        </a>
        .
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
        <a
          href="https://www.gov.uk/government/publications/standards-for-official-statistics-published-by-the-department-for-education"
          rel="noopener noreferrer nofollow"
          target="_blank"
        >
          Standards for official statistics published by DfE guidance (opens in
          new tab)
        </a>
        .
      </p>
      <p>
        Our statistical practice is regulated by the Office for Statistics
        Regulation (OSR).
      </p>
      <p>
        OSR sets the standards of trustworthiness, quality and value in the{' '}
        <a
          href="https://code.statisticsauthority.gov.uk/the-code/"
          rel="noopener noreferrer nofollow"
          target="_blank"
        >
          Code of Practice for Statistics (opens in new tab)
        </a>{' '}
        that all producers of official statistics should adhere to.
      </p>
      <p>
        You are welcome to contact us directly with any comments about how we
        meet these standards. Alternatively, you can contact OSR by emailing{' '}
        <a href="mailto:regulation@statistics.gov.uk">
          regulation@statistics.gov.uk
        </a>{' '}
        or via the{' '}
        <a
          href="https://osr.statisticsauthority.gov.uk/"
          rel="noopener noreferrer nofollow"
          target="_blank"
        >
          OSR website (opens in new tab)
        </a>
        .
      </p>
    </>
  );
}
