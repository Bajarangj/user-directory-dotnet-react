import { useState } from 'react';
import type { FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { createUser } from '../api/usersApi';
import { useAuth } from '../auth/AuthProvider';

type FormValues = {
  name: string;
  age: string;
  city: string;
  state: string;
  pincode: string;
};

type FormErrors = Partial<Record<keyof FormValues, string>>;

const initialValues: FormValues = { name: '', age: '', city: '', state: '', pincode: '' };

function validate(values: FormValues): FormErrors {
  const errors: FormErrors = {};
  const name = values.name.trim();
  const age = Number(values.age);

  if (name.length < 2 || name.length > 100) {
    errors.name = 'Name is required and must be between 2 and 100 characters.';
  }
  if (!Number.isInteger(age) || age < 0 || age > 120) {
    errors.age = 'Age is required and must be an integer from 0 to 120.';
  }
  if (!values.city.trim()) errors.city = 'City is required.';
  if (!values.state.trim()) errors.state = 'State is required.';
  if (values.pincode.trim().length < 4 || values.pincode.trim().length > 10) {
    errors.pincode = 'Pincode is required and must be between 4 and 10 characters.';
  }

  return errors;
}

export function AddUserPage() {
  const { getToken } = useAuth();
  const navigate = useNavigate();
  const [values, setValues] = useState(initialValues);
  const [errors, setErrors] = useState<FormErrors>({});
  const [serverError, setServerError] = useState<string>();
  const [isSubmitting, setIsSubmitting] = useState(false);

  function updateValue(field: keyof FormValues, value: string) {
    setValues(current => ({ ...current, [field]: value }));
    setErrors(current => ({ ...current, [field]: undefined }));
    setServerError(undefined);
  }

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const validationErrors = validate(values);
    setErrors(validationErrors);
    setServerError(undefined);

    if (Object.keys(validationErrors).length > 0) return;

    setIsSubmitting(true);
    try {
      await createUser(getToken, {
        name: values.name.trim(),
        age: Number(values.age),
        city: values.city.trim(),
        state: values.state.trim(),
        pincode: values.pincode.trim(),
      });
      navigate('/users', { replace: true, state: { success: 'User added successfully.' } });
    } catch (exception) {
      setServerError(exception instanceof Error ? exception.message : 'Unable to create user.');
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <section className="page-card form-card">
      <div className="page-heading">
        <div>
          <p className="eyebrow">Directory</p>
          <h1>Add user</h1>
        </div>
      </div>

      {serverError && <div className="alert error" role="alert">{serverError}</div>}

      <form className="user-form" onSubmit={submit} noValidate>
        <div className="form-grid">
          <Field label="Name" name="name" value={values.name} error={errors.name} onChange={value => updateValue('name', value)} />
          <Field label="Age" name="age" type="number" value={values.age} error={errors.age} onChange={value => updateValue('age', value)} />
          <Field label="City" name="city" value={values.city} error={errors.city} onChange={value => updateValue('city', value)} />
          <Field label="State" name="state" value={values.state} error={errors.state} onChange={value => updateValue('state', value)} />
          <Field label="Pincode" name="pincode" value={values.pincode} error={errors.pincode} onChange={value => updateValue('pincode', value)} />
        </div>
        <div className="form-actions">
          <button className="button primary" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Saving...' : 'Save user'}
          </button>
        </div>
      </form>
    </section>
  );
}

function Field({
  label,
  name,
  value,
  error,
  type = 'text',
  onChange,
}: {
  label: string;
  name: string;
  value: string;
  error?: string;
  type?: string;
  onChange: (value: string) => void;
}) {
  return (
    <div className="field">
      <label htmlFor={name}>{label}</label>
      <input
        id={name}
        name={name}
        type={type}
        value={value}
        min={type === 'number' ? 0 : undefined}
        max={type === 'number' ? 120 : undefined}
        onChange={event => onChange(event.target.value)}
        aria-invalid={Boolean(error)}
        aria-describedby={error ? `${name}-error` : undefined}
      />
      {error && <span className="field-error" id={`${name}-error`}>{error}</span>}
    </div>
  );
}
