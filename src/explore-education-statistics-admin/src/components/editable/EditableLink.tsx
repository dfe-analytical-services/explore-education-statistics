import Link, { LinkProps } from '@admin/components/Link';
import Button from '@common/components/Button';
import { useEditingContext } from '@common/contexts/EditingContext';
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
  const { isEditing } = useEditingContext();

  return !isEditing ? (
    <Link
      to=""
      href={href}
      {...props}
      rel="noopener noreferrer"
      target="_blank"
    >
      {children}
    </Link>
  ) : (
    <div>
      <div>
        <Link
          to=""
          href={href}
          {...props}
          rel="noopener noreferrer"
          target="_blank"
        >
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
