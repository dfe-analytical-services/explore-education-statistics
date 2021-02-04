import Page from '@admin/components/Page';
import methodologyService, {
  MethodologyStatusListItem,
} from '@admin/services/methodologyService';
import Tag from '@common/components/Tag';
import React, { useEffect, useState } from 'react';

interface Model {
  methodologies: MethodologyStatusListItem[];
}

const BauMethodologyPage = () => {
  const [model, setModel] = useState<Model>();

  useEffect(() => {
    methodologyService.getMyMethodologies().then(methodologies => {
      setModel({
        methodologies,
      });
    });
  }, []);

  return (
    <Page
      title="Methodology status"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Methodology' },
      ]}
    >
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
                  <Tag strong>{methodology.status}</Tag>
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

export default BauMethodologyPage;
