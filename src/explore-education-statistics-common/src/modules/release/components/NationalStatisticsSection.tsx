import React from 'react';

export default function NationalStatisticsSection({
  showHeading = true,
}: {
  showHeading?: boolean;
}) {
  return (
    <>
      {showHeading && <h3>National statistics</h3>}
      <p>
        These accredited official statistics have been independently reviewed by
        the{' '}
        <a href="https://osr.statisticsauthority.gov.uk/what-we-do/">
          Office for Statistics Regulation
        </a>{' '}
        (OSR). They comply with the standards of trustworthiness, quality and
        value in the{' '}
        <a href="https://code.statisticsauthority.gov.uk/the-code/">
          Code of Practice for Statistics
        </a>
        . Accredited official statistics are called National Statistics in the{' '}
        <a href="https://www.legislation.gov.uk/ukpga/2007/18/contents">
          Statistics and Registration Service Act 2007
        </a>
        .
      </p>
      <p>
        Accreditation signifies their compliance with the authority's{' '}
        <a href="https://code.statisticsauthority.gov.uk/the-code/">
          Code of Practice for Statistics
        </a>{' '}
        which broadly means these statistics are:
      </p>
      <ul className="govuk-list govuk-list--bullet">
        <li>managed impartially and objectively in the public interest</li>
        <li>meet identified user needs</li>
        <li>produced according to sound methods</li>
        <li>well explained and readily accessible</li>
      </ul>
      <p>
        Our statistical practice is regulated by the Office for Statistics
        Regulation (OSR).
      </p>
      <p>
        OSR sets the standards of trustworthiness, quality and value in the{' '}
        <a href="https://code.statisticsauthority.gov.uk/the-code/">
          Code of Practice for Statistics
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
        <a href="https://osr.statisticsauthority.gov.uk/">OSR website</a>.
      </p>
    </>
  );
}
