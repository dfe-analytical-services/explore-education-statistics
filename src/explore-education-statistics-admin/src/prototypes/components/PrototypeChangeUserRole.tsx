import classNames from 'classnames';
import Details from '@common/components/Details';
import React, { useState } from 'react';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import FormEditor from '@admin/components/form/FormEditor';
import FormFieldCheckbox from '@common/components/form/FormFieldCheckbox';
import Button from '@common/components/Button';
import styles from '../PrototypePublicPage.module.scss';
import PrototypeDownloadAncillaryLinks from './PrototypeDownloadAncillaryLinks';

interface Props {
  selectedRole?: boolean;
  lastItem?: string;
  name?: string;
  release?: string;
  roleId?: string;
  className?: string;
  type?: string;
}

const roles = ['Approver', 'Contributor', 'Lead', 'Viewer', 'No access'];

const PrototypeSelectedRole = ({
  selectedRole,
  lastItem,
  name,
  release,
  roleId,
  className,
  type,
}: Props) => {
  const [showRoleModal, toggleRoleModal] = useToggle(false);
  const [roleType, setRoleType] = useState(selectedRole);

  //let role = '';

  return (
    <div
      className={classNames(
        'dfe-flex dfe-justify-content--space-between dfe-align-items--center dfe-flex-grow--1 govuk-!-padding-right-9',
        className,
      )}
    >
      {/*<label htmlFor={`role-${roleId}`} className="govuk-!-margin-right-3">{release}</label>
      <select 
        className="govuk-select" 
        name="role" 
        id={`role-${roleId}`}
        onBlur={event => {
          role = event.target.value;
          setRoleType(role);
        }}
      >
        <>
          {roles.map((item, index) => (
            <option key={index.toString()} selected={selectedRole === item}>
              {item}
            </option>
          ))}
        </>
          </select>*/}

      {/*<div className="govuk-checkboxes">
        <div className="govuk-checkboxes__item">
          <input type="checkbox" className="govuk-checkboxes__input" name='role' id={`role-${roleId}`} checked={selectedRole} />
          <label htmlFor={`role-${roleId}`} className="govuk-label govuk-checkboxes__label">Grant access</label>
        </div>
        </div>*/}

      <p
        className={classNames(
          'govuk-!-margin-0',
          'govuk-tag',
          'dfe-flex-basis--45',
          'dfe-align--centre',
          roleType ? 'govuk-tag--grey' : 'govuk-tag--red',
        )}
      >
        {roleType ? 'Access Granted' : 'No access'}
      </p>

      {!roleType && (
        <Button
          onClick={() => {
            toggleRoleModal(true);
            setRoleType(true);
          }}
          type="button"
          variant="secondary"
          className="govuk-!-margin-top-2 govuk-!-margin-bottom-2 dfe-flex-basis--45"
        >
          Grant access
        </Button>
      )}
      {roleType && (
        <Button
          onClick={() => {
            toggleRoleModal(true);
            setRoleType(false);
          }}
          type="button"
          variant="warning"
          className="govuk-!-margin-top-2 govuk-!-margin-bottom-2 dfe-flex-basis--45"
        >
          Remove access
        </Button>
      )}

      {/*!type && (
        <button
          onClick={() => {
            toggleRoleModal(true);
          }}
          type="button" 
          className={
            classNames(
              'govuk-button', 
              'govuk-button--secondary', 
              'govuk-!-margin-left-2',
              {
                'govuk-!-margin-bottom-9': lastItem, 
                'govuk-!-margin-bottom-2': !lastItem
              })
          }
        >
          Set role
        </button>
        )}*/}
      <ModalConfirm
        open={showRoleModal}
        title={`Change access for ${name}`}
        onExit={() => toggleRoleModal(false)}
        onConfirm={() => toggleRoleModal(false)}
        onCancel={() => toggleRoleModal(false)}
      >
        <p>
          Are you sure you want to{' '}
          <strong>{roleType ? 'grant access' : 'remove access'}</strong> <br />
          for <strong>{release}</strong>?
        </p>
      </ModalConfirm>
    </div>
  );
};

export default PrototypeSelectedRole;
