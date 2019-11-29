import React, { useContext, useEffect, useState } from 'react';
import methodologyService from '@admin/services/methodology/service';
import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import RelatedInformation from '@common/components/RelatedInformation';
import { MethodologyStatus } from '@admin/services/methodology/types';

interface Model {
  methodologies: MethodologyStatus[];
}

const ListMethodologyPages = () => {
  const [model, setModel] = useState<Model>();

  useEffect(() => {
    methodologyService.getMethodologies().then(methodologies => {
      setModel({
        methodologies,
      });
    });
  }, []);

  return (
    <Page wide breadcrumbs={[{ name: 'Manage methodology' }]}>
      <div className="govuk-grid-row">
        <div className="govuk-grid-column-two-thirds">
          <h1 className="govuk-heading-xl">Manage methodology</h1>
        </div>
        <div className="govuk-grid-column-one-third">
          <RelatedInformation heading="Help and guidance">
            <ul className="govuk-list">
              <li>
                <Link to="/documentation" target="blank">
                  Creating new methodology{' '}
                </Link>
              </li>
            </ul>
          </RelatedInformation>
        </div>
      </div>

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
                <td className="govuk-table__header">
                  <Link to={`/methodology/${methodology.id}`}>{methodology.title}</Link>
                </td>
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

      <Link to="/methodology/create" className="govuk-button">
        Create new methodology
      </Link>
    </Page>
  );
};

export default ListMethodologyPages;
