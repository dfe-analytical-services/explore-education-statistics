import Link from '@admin/components/Link';
import Page from '@admin/components/Page';
import methodologyService from '@admin/services/methodology/service';
import { MethodologyStatus } from '@admin/services/methodology/types';
import RelatedInformation from '@common/components/RelatedInformation';
import Tabs from '@common/components/Tabs';
import TabsSection from '@common/components/TabsSection';
import React, { useEffect, useState } from 'react';

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

      <Tabs id="methodologyTabs">
        <TabsSection id="manage-methodology" title="Manage methodology">
          {model && (
            <table className="govuk-table">
              <thead className="govuk-table__head">
                <tr className="govuk-table__row">
                  <th scope="col" className="govuk-table__header">
                    Methodology title
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Status
                  </th>
                  <th scope="col" className="govuk-table__header">
                    Related publications
                  </th>
                </tr>
              </thead>
              <tbody className="govuk-table__body">
                {model.methodologies.map(methodology => (
                  <tr className="govuk-table__row" key={methodology.id}>
                    <td className="govuk-table__header">
                      <Link to={`/methodology/${methodology.id}`}>
                        {methodology.title}
                      </Link>
                    </td>
                    <td className="govuk-table__cell">
                      <strong className="govuk-tag">
                        {methodology.status}
                      </strong>
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
        </TabsSection>
        <TabsSection id="draft-methodology" title="Draft methodology (0)">
          <div className="govuk-inset-text">
            There is currently no draft methodology
          </div>
        </TabsSection>
      </Tabs>
    </Page>
  );
};

export default ListMethodologyPages;
