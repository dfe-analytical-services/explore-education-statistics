import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import React, { useEffect, useState } from 'react';
import methodologyService from '@admin/services/methodology/service';
import Page from '@admin/components/Page';
import { MethodologyStatusListItem } from '@admin/services/methodology/types';

interface Model {
  methodologies: MethodologyStatusListItem[];
}

const BauMethodologyPage = ({ handleApiErrors }: ErrorControlProps) => {
  const [model, setModel] = useState<Model>();

  useEffect(() => {
    methodologyService
      .getMethodologies()
      .then(methodologies => {
        setModel({
          methodologies,
        });
      })
      .catch(handleApiErrors);
  }, [handleApiErrors]);

  return (
    <Page
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Methodology' },
      ]}
    >
      <h1>Methodology status</h1>

      {model && (
        <table className="govuk-table">
          <caption className="govuk-table__caption">
            Current methodologies
          </caption>
          <thead className="govuk-table__head">
            <tr className="govuk-table__row">
              <th scope="col" className="govuk-table__header">
                Methodology
              </th>
              <th scope="col" className="govuk-table__header">
                Status
              </th>
              <th scope="col" className="govuk-table__header">
                Publications
              </th>
            </tr>
          </thead>
          <tbody className="govuk-table__body">
            {model.methodologies.map(methodology => (
              <tr className="govuk-table__row" key={methodology.id}>
                <td className="govuk-table__header">{methodology.title}</td>
                <td className="govuk-table__cell">
                  <strong className="govuk-tag">{methodology.status}</strong>
                </td>
                <td className="govuk-table__cell">
                  <ul className="govuk-list">
                    {methodology.publications.map(publication => (
                      <li key={publication.id}>{publication.title}</li>
                    ))}
                  </ul>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </Page>
  );
};

export default withErrorControl(BauMethodologyPage);
