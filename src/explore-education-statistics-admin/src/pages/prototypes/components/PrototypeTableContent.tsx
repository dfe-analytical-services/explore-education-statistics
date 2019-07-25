import {
  FormFieldset,
  FormGroup,
  FormTextInput,
} from '@common/components/form';
import Link from '@admin/components/Link';
import React from 'react';
import { PrototypeTable } from '@admin/pages/prototypes/PrototypeData';

const PrototypeTableContent = ({
  task,
  table,
}: {
  task?: string;
  table?: PrototypeTable;
}) => {
  const title =
    (table && table.title) ||
    "'Absence by characteristic' from 'Pupil absence' in England between 2012/13 and 2016/17";

  return (
    <>
      <table className="govuk-table">
        <caption className="govuk-table__caption govuk-!-margin-bottom-6">
          {title}
        </caption>
        <thead className="govuk-table__head">
          <tr>
            <th colSpan={2} />
            <th scope="col" className="govuk-table__header--numeric">
              2012/13
            </th>
            <th scope="col" className="govuk-table__header--numeric">
              2013/14
            </th>
            <th scope="col" className="govuk-table__header--numeric">
              2014/15
            </th>
            <th scope="col" className="govuk-table__header--numeric">
              2015/16
            </th>
            <th scope="col" className="govuk-table__header--numeric">
              2016/17
            </th>
          </tr>
        </thead>
        <tbody className="govuk-table__body">
          <tr>
            <th rowSpan={4} scope="row">
              All schools
            </th>
            <th scope="row">Number of pupil enrolments</th>
            <td className="govuk-table__cell--numeric">6,477,725</td>
            <td className="govuk-table__cell--numeric">6,554,005</td>
            <td className="govuk-table__cell--numeric">6,642,755</td>
            <td className="govuk-table__cell--numeric">6,737,190</td>
            <td className="govuk-table__cell--numeric">6,899,770</td>
          </tr>
          <tr>
            <th scope="row">Authorised absence rate</th>
            <td className="govuk-table__cell--numeric">4.2%</td>
            <td className="govuk-table__cell--numeric">3.5%</td>
            <td className="govuk-table__cell--numeric">3.5%</td>
            <td className="govuk-table__cell--numeric">3.4%</td>
            <td className="govuk-table__cell--numeric">3.4%</td>
          </tr>
          <tr>
            <th scope="row">Unuthorised absence rate</th>
            <td className="govuk-table__cell--numeric">1.1%</td>
            <td className="govuk-table__cell--numeric">1.1%</td>
            <td className="govuk-table__cell--numeric">1.1%</td>
            <td className="govuk-table__cell--numeric">1.1%</td>
            <td className="govuk-table__cell--numeric">1.3%</td>
          </tr>
          <tr>
            <th scope="row">Overall absence rate</th>
            <td className="govuk-table__cell--numeric">5.3%</td>
            <td className="govuk-table__cell--numeric">4.5%</td>
            <td className="govuk-table__cell--numeric">4.6%</td>
            <td className="govuk-table__cell--numeric">4.6%</td>
            <td className="govuk-table__cell--numeric">4.7%</td>
          </tr>
        </tbody>
      </table>
      <p className="govuk-body-s">Source: DfE prototype example statistics</p>
    </>
  );
};

export default PrototypeTableContent;
