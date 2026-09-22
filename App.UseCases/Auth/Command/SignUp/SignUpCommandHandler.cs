using App.Domain.User.Entities;
using App.Interfaces.Ports;
using App.Interfaces.Ports.User;
using App.Objects.User.DTOs.Input.Command;
using App.Shared.Result;
using App.Shared.Security;
using App.Shared.Validation;
using Cortex.Mediator.Commands;

namespace App.UseCases.Auth.Command.SignUp;

public class SignUpCommandHandler : ICommandHandler<SignUpCommand, OutputPort<Guid>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IInputValidator<SignUpDto> _validator;
    
    public SignUpCommandHandler(
        IUserRepository userRepository, 
        IInputValidator<SignUpDto> validator, 
        IPasswordHasher passwordHasher, 
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _validator = validator;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<OutputPort<Guid>> Handle(SignUpCommand command, CancellationToken cancellationToken)
    {
        if (!await _validator.ValidateAsync(command.Input, cancellationToken))
        {
            return OutputPort<Guid>.Failure(_validator.StatusCode, _validator.Messages.ToArray());
        }
        
        var dto = command.Input;
        
        var hashedPassword = _passwordHasher.HashPassword(dto.Password);
        
        var user = TUser.Create(
            dto.Email, 
            dto.Username, 
            hashedPassword,
            dto.FirstName,
            dto.LastName,
            dto.DateOfBirth
        );
        
        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);
        
        return OutputPort<Guid>.Success(user.Id, System.Net.HttpStatusCode.Created, "User registered successfully");
    }
}
