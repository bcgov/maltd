/* eslint-disable react/jsx-filename-extension */
import React from 'react';
import { storiesOf } from '@storybook/react';
import { action } from '@storybook/addon-actions';
import 'bootstrap/dist/css/bootstrap.css';

import InputField from './InputField';

export const inputField = {
  name: 'idir',
  type: 'text',
  placeholder: 'IDIR/username',
  valid: false,
  invalid: false,
  value: '',
};

export const actions = {
  onUpdate: action('onUpdate'),
};

/* eslint-disable react/jsx-props-no-spreading */
storiesOf('InputField', module)
  .add('default', () => <InputField inputField={inputField} {...actions} />)
  .add('valid', () => <InputField inputField={{ ...inputField, valid: true, value: 'IDIR/testuser' }} {...actions} />)
  .add('invalid', () => <InputField inputField={{ ...inputField, invalid: true, value: 'a' }} {...actions} />);
