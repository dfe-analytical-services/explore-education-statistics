import Link, { LinkProps } from '@admin/components/Link';
import { useEditingContext } from '@admin/contexts/EditingContext';
import Button from '@common/components/Button';
import React from 'react';

interface Props extends LinkProps {
  to: string;
  removeOnClick: () => void;
}

const EditableLink = ({
  removeOnClick,
  to: href,
  children,
  ...props
}: Props) => {
  const { editingMode } = useEditingContext();

  return editingMode !== 'edit' ? (
    <Link {...props} to={href}>
      {children}
    </Link>
  ) : (
    <div>
      <div>
        <Link {...props} to={href}>
          {children}
        </Link>
      </div>
      <Button
        className="govuk-!-margin-bottom-3"
        variant="secondary"
        onClick={() => removeOnClick()}
      >
        Remove link
      </Button>
    </div>
  );
};

export default EditableLink;
