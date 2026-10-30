import Link from '@admin/components/Link';
import { ReplacementPairedIndicator } from '@admin/pages/release/data/utils/replacementPairedIndicators';
import Tag from '@common/components/Tag';
import React from 'react';

interface Props {
  manageIndicatorsPath: string;
  pairedIndicators: ReplacementPairedIndicator[];
}

/**
 * Shows which paired indicators will be maintained or dropped by a data
 * replacement.
 */
export default function DataFileReplacementPairedIndicators({
  manageIndicatorsPath,
  pairedIndicators,
}: Props) {
  const hasDroppedPairs = pairedIndicators.some(pair => !pair.maintained);

  return (
    <>
      <h3 className="govuk-heading-m govuk-!-padding-top-4">
        Paired indicators
      </h3>

      {pairedIndicators.length > 0 ? (
        <>
          <p>
            {hasDroppedPairs
              ? 'One or more paired indicators will be dropped by this data replacement because their parent or child indicator has not been mapped. Indicators from a dropped pair can be selected independently in the table tool.'
              : 'All paired indicators will be maintained by this data replacement.'}
          </p>

          <table data-testid="paired-indicators-table">
            <caption className="govuk-visually-hidden">
              Table showing whether each pair of indicators will be maintained
              or dropped by this data replacement.
            </caption>
            <thead>
              <tr>
                <th scope="col">Parent indicator</th>
                <th scope="col">Child indicator</th>
                <th scope="col">Status</th>
              </tr>
            </thead>
            <tbody>
              {pairedIndicators.map(pair => (
                <tr key={`${pair.parentIndicatorId}-${pair.childIndicatorId}`}>
                  <td>{pair.parentLabel}</td>
                  <td>{pair.childLabel}</td>
                  <td>
                    <Tag colour={pair.maintained ? 'green' : 'orange'}>
                      {pair.maintained ? 'Maintained' : 'Dropped'}
                    </Tag>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </>
      ) : (
        <p>No indicators have been paired for this data set.</p>
      )}

      <p>
        <Link to={manageIndicatorsPath} unvisited>
          Manage paired indicators
        </Link>
      </p>
    </>
  );
}
