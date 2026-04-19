










using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;


























































public partial class @CipherInputActions: IInputActionCollection2, IDisposable
{
    
    
    
    public InputActionAsset asset { get; }

    
    
    
    public @CipherInputActions()
    {
        asset = InputActionAsset.FromJson(@"{
    ""version"": 1,
    ""name"": ""CipherInputActions"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""34414b57-dfa1-4cbf-be30-9290e8cc56cf"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""84365548-8a08-46c5-b26b-0f2f41a957db"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": true
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""2206f62a-0e88-4fa7-9d14-10fad7e2f24b"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Slide"",
                    ""type"": ""Button"",
                    ""id"": ""813ea02d-8b9a-4842-a56a-85d24bd4a3da"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Gadget1"",
                    ""type"": ""Button"",
                    ""id"": ""2407af8d-a8a1-4cda-b2d1-ebabbeb781f1"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Gadget2"",
                    ""type"": ""Button"",
                    ""id"": ""62e8a947-bcf9-4b96-9dfc-f3ea4b767032"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                },
                {
                    ""name"": ""Gadget3"",
                    ""type"": ""Button"",
                    ""id"": ""2369a1c2-cf78-4387-b9cd-15bf83a902cf"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """",
                    ""initialStateCheck"": false
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""WASD"",
                    ""id"": ""6874efc9-31d7-453a-b439-09f9a6fe1d77"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""d4847898-3f87-4c79-b928-2bfd9bd5019a"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""cf524bf0-f27f-4199-84e5-2a9577bcf44e"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""33fe272e-c917-48d5-a228-27ca5bfa809b"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""0adc5648-bccd-4537-85e9-6de3e403b1dd"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""ArrowKeys"",
                    ""id"": ""5cc505b5-a14b-420d-9a02-f392af2c8651"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""9d79fc88-6a90-4d98-a2d0-dfbc84a60c64"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""7830e500-2a03-4358-9ce1-96354797eabd"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""40cffd39-0d14-48ec-b523-1606ae03706a"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""79846490-66a3-4891-8a5b-60f8305661a6"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""f79582c9-1e13-4686-8398-e1a5c720aa7f"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""cc6ad754-5af2-4545-a98b-53e62bfd61d6"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Slide"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""b9ff2a0b-c1b3-4c7f-8191-06493e821ce0"",
                    ""path"": ""<Keyboard>/q"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Gadget1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""2ff88a8e-f440-4352-8cab-1ffd8faa1fed"",
                    ""path"": ""<Keyboard>/e"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Gadget2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""499e9f9a-5fcf-4ae7-be30-cd588f4c2470"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Gadget3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": []
}");
        
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
        m_Player_Slide = m_Player.FindAction("Slide", throwIfNotFound: true);
        m_Player_Gadget1 = m_Player.FindAction("Gadget1", throwIfNotFound: true);
        m_Player_Gadget2 = m_Player.FindAction("Gadget2", throwIfNotFound: true);
        m_Player_Gadget3 = m_Player.FindAction("Gadget3", throwIfNotFound: true);
    }

    ~@CipherInputActions()
    {
        UnityEngine.Debug.Assert(!m_Player.enabled, "This will cause a leak and performance issues, CipherInputActions.Player.Disable() has not been called.");
    }

    
    
    
    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    
    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    
    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    
    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    
    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    
    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    
    public void Enable()
    {
        asset.Enable();
    }

    
    public void Disable()
    {
        asset.Disable();
    }

    
    public IEnumerable<InputBinding> bindings => asset.bindings;

    
    public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
    {
        return asset.FindAction(actionNameOrId, throwIfNotFound);
    }

    
    public int FindBinding(InputBinding bindingMask, out InputAction action)
    {
        return asset.FindBinding(bindingMask, out action);
    }

    
    private readonly InputActionMap m_Player;
    private List<IPlayerActions> m_PlayerActionsCallbackInterfaces = new List<IPlayerActions>();
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_Slide;
    private readonly InputAction m_Player_Gadget1;
    private readonly InputAction m_Player_Gadget2;
    private readonly InputAction m_Player_Gadget3;
    
    
    
    public struct PlayerActions
    {
        private @CipherInputActions m_Wrapper;

        
        
        
        public PlayerActions(@CipherInputActions wrapper) { m_Wrapper = wrapper; }
        
        
        
        public InputAction @Move => m_Wrapper.m_Player_Move;
        
        
        
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        
        
        
        public InputAction @Slide => m_Wrapper.m_Player_Slide;
        
        
        
        public InputAction @Gadget1 => m_Wrapper.m_Player_Gadget1;
        
        
        
        public InputAction @Gadget2 => m_Wrapper.m_Player_Gadget2;
        
        
        
        public InputAction @Gadget3 => m_Wrapper.m_Player_Gadget3;
        
        
        
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        
        public void Enable() { Get().Enable(); }
        
        public void Disable() { Get().Disable(); }
        
        public bool enabled => Get().enabled;
        
        
        
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        
        
        
        
        
        
        
        
        public void AddCallbacks(IPlayerActions instance)
        {
            if (instance == null || m_Wrapper.m_PlayerActionsCallbackInterfaces.Contains(instance)) return;
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Add(instance);
            @Move.started += instance.OnMove;
            @Move.performed += instance.OnMove;
            @Move.canceled += instance.OnMove;
            @Jump.started += instance.OnJump;
            @Jump.performed += instance.OnJump;
            @Jump.canceled += instance.OnJump;
            @Slide.started += instance.OnSlide;
            @Slide.performed += instance.OnSlide;
            @Slide.canceled += instance.OnSlide;
            @Gadget1.started += instance.OnGadget1;
            @Gadget1.performed += instance.OnGadget1;
            @Gadget1.canceled += instance.OnGadget1;
            @Gadget2.started += instance.OnGadget2;
            @Gadget2.performed += instance.OnGadget2;
            @Gadget2.canceled += instance.OnGadget2;
            @Gadget3.started += instance.OnGadget3;
            @Gadget3.performed += instance.OnGadget3;
            @Gadget3.canceled += instance.OnGadget3;
        }

        
        
        
        
        
        
        
        private void UnregisterCallbacks(IPlayerActions instance)
        {
            @Move.started -= instance.OnMove;
            @Move.performed -= instance.OnMove;
            @Move.canceled -= instance.OnMove;
            @Jump.started -= instance.OnJump;
            @Jump.performed -= instance.OnJump;
            @Jump.canceled -= instance.OnJump;
            @Slide.started -= instance.OnSlide;
            @Slide.performed -= instance.OnSlide;
            @Slide.canceled -= instance.OnSlide;
            @Gadget1.started -= instance.OnGadget1;
            @Gadget1.performed -= instance.OnGadget1;
            @Gadget1.canceled -= instance.OnGadget1;
            @Gadget2.started -= instance.OnGadget2;
            @Gadget2.performed -= instance.OnGadget2;
            @Gadget2.canceled -= instance.OnGadget2;
            @Gadget3.started -= instance.OnGadget3;
            @Gadget3.performed -= instance.OnGadget3;
            @Gadget3.canceled -= instance.OnGadget3;
        }

        
        
        
        
        public void RemoveCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterfaces.Remove(instance))
                UnregisterCallbacks(instance);
        }

        
        
        
        
        
        
        
        
        
        public void SetCallbacks(IPlayerActions instance)
        {
            foreach (var item in m_Wrapper.m_PlayerActionsCallbackInterfaces)
                UnregisterCallbacks(item);
            m_Wrapper.m_PlayerActionsCallbackInterfaces.Clear();
            AddCallbacks(instance);
        }
    }
    
    
    
    public PlayerActions @Player => new PlayerActions(this);
    
    
    
    
    
    public interface IPlayerActions
    {
        
        
        
        
        
        
        void OnMove(InputAction.CallbackContext context);
        
        
        
        
        
        
        void OnJump(InputAction.CallbackContext context);
        
        
        
        
        
        
        void OnSlide(InputAction.CallbackContext context);
        
        
        
        
        
        
        void OnGadget1(InputAction.CallbackContext context);
        
        
        
        
        
        
        void OnGadget2(InputAction.CallbackContext context);
        
        
        
        
        
        
        void OnGadget3(InputAction.CallbackContext context);
    }
}
