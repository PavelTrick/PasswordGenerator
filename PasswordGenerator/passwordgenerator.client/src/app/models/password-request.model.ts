export interface PasswordRequest {
    amount: number;
    length: number;
    includeSpecial: boolean;
    includeNumbers: boolean;
    includeUppercase: boolean;
    includeLowercase: boolean;
    useSimpleGenerator: boolean;
  }