import { AutoMappedLocation } from '@admin/pages/release/data/utils/getApiDataSetLocationMappings';
import styles from '@admin/pages/release/data/ReleaseApiDataSetLocationsMappingPage.module.scss';
import ButtonText from '@common/components/ButtonText';
import Tag from '@common/components/Tag';
import VisuallyHidden from '@common/components/VisuallyHidden';
import React from 'react';

interface Props {
  locations: AutoMappedLocation[];
}

export default function ApiDataSetAutoMappedLocationsTable({
  locations,
}: Props) {
  return (
    <table className={styles.table} data-testid="auto-mapped">
      <caption className="govuk-visually-hidden">
        Table showing all auto mapped locations
      </caption>
      <thead>
        <tr>
          <th className="govuk-!-width-one-third">Current data set</th>
          <th className="govuk-!-width-one-third">New data set</th>
          <th>Type</th>
          <th>Actions</th>
        </tr>
      </thead>
      <tbody>
        {/* TODO EES-5325 add search and pagination, just showing first ten for now. */}
        {locations.slice(0, 10).map(({ candidate, mapping }) => {
          return (
            <tr key={`mapping-${mapping.source.code}`}>
              <td>
                {mapping.source.label}
                <br />
                <span className={styles.code}>{mapping.source.code}</span>
              </td>
              <td>
                {candidate.label}
                <br />
                <span className={styles.code}>{candidate.code}</span>
              </td>
              <td>
                <Tag colour="grey">Minor</Tag>
              </td>
              <td>
                <ButtonText>
                  Edit
                  <VisuallyHidden>
                    {' '}
                    mapping for {mapping.source.label}
                  </VisuallyHidden>
                </ButtonText>
              </td>
            </tr>
          );
        })}
      </tbody>
    </table>
  );
}
